using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GeocodingAPI.Data;
using GeocodingAPI.Middlewares;
using GeocodingAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;

namespace GeocodingAPI.Services
{
    public interface IGeocodingServices {
        Task<IResult> ValidationProcessing(string responseValue);
    };
    public class GeocodingServices : IGeocodingServices
    {
        GeocodingAPIDbContext _DbContext;
        ConcurrentDictionary<byte[], Lazy<Task<GeoCodeResponse>>> _addressResponses;
        Dictionary<string, int> _provinces;
        HttpClient _httpClient;
        ILogger<GeocodingServices> _logger;

        public GeocodingServices(GeocodingAPIDbContext context, HttpClient httpClient, ILogger<GeocodingServices> logger)
        {
            _DbContext = context;
            _addressResponses = new ConcurrentDictionary<byte[], Lazy<Task<GeoCodeResponse>>>();
            GetProvinceValue(null);
            _httpClient = httpClient;
            _logger = logger;
        }


        /// <summary>
        /// ValidationProcessing accepts one or many addresses and returns their corresponding geocoding data, 
        /// 1. It processes each address and transform it to structured address
        /// 2. Checks for it's geocoding data in DB, if present in DB then returns the same.
        /// 3. If not present in DB then calls Nominatim API. If it returns proper response then stores for the future use and returns the same to user.
        /// </summary>
        /// <param name="responseValue">Incoming request, may contain one or many addresses, seperated by comma and each address is included in {}</param>
        /// <returns>Instance of Result with object of UserResponse as value </returns>
        /// <exception cref="BadRequestException"></exception>
        public async Task<IResult> ValidationProcessing(string responseValue)
        {
            List<UserResponse> userResponses = new List<UserResponse>();
            List<UserRequestEachAddress> ureaList = null;

            #region Replacing '"' and removing unnecessary space from input string 
            string addressString = responseValue.ToString().Trim();
            addressString = Regex.Replace(addressString, @"[""]", "");
            #endregion


            #region Check incoming request whether it is valid or invalid
            if (!IsValid(addressString))
            {
                throw new BadRequestException($"Invalid address: {addressString}");
            }
            #endregion

            // Check if incoming user query is present in DB
            await AddToUserRequest(addressString);
            _logger.LogInformation("User request saved in DB.");

            // Request may have one or many addresses, splitting the request into each address to ease in searching their geocoding data.
            List<string> result = SplitAddress(addressString);

            // Check and add split results into UserRequestEachAddress
            ureaList = await AddUserRequestEachAddress(result);

            // Once we got the list of addresses of all new requests,
            // next step will be transform those into CanadianAddresses by  Normalizing it
            // Below are the guideline for transformations
            // Remove "Apt. / Apt, Unit, #, Suite, dash-prefixed unit nos. (123-12 Main st -> 123 Main st)
            List<UserRequestEachAddress> iur = await NormalizeRawAddress(ureaList);

            
            // As we have list of UserRequestEachAddress objects to search for their curresponsing geocodings
            if (iur != null && iur.Count > 0)
            {
                UserResponse ur;

                #region If Geocoding response for the incoming address query is not present then calling External API
                foreach (UserRequestEachAddress item in iur)
                {
                    ur = new UserResponse();
                    if (item.GeoCodeRequests != null)
                    {
                        if (item.GeoCodeRequests.GeoCodeResponse == null)
                        {
                            var resp = await GetCurrespondingResponseFromWebAPI(item.GeoCodeRequests);
                            if (resp != null && resp is GeoCodeResponse)
                            {
                                ur.GeoCodeResponse = resp as GeoCodeResponse;
                                ur.UserQuerryAddress = item.EachAddress;
                                userResponses.Add(ur);
                            }
                            else
                            {
                                ur.UserResponseString = "Data Not Found";
                                ur.UserQuerryAddress = item.EachAddress;
                                userResponses.Add(ur);
                            }
                        }
                        else
                        {
                            ur.GeoCodeResponse = item.GeoCodeRequests.GeoCodeResponse;
                            ur.UserQuerryAddress = item.EachAddress;
                            userResponses.Add(ur);
                        }
                    }
                    else
                    {
                        ur.UserResponseString = "Data Not Found";
                        ur.UserQuerryAddress = item.EachAddress;
                        userResponses.Add(ur);
                    }
                }
                #endregion

                return Results.Ok(userResponses);
            }
            else
            {
                _logger.LogInformation($"Unable to get geocoding data for {addressString}");
                return Results.Problem(GeoCodeErrors.NotFound);
            }
        }

        private async Task<GeoCodeResponse> GetCurrespondingResponseFromWebAPI(GeoCodeRequest reqAdd)
        {
            CanadianAddress req = reqAdd.CanadianAddress;
            var resp = _addressResponses.GetOrAdd(req.HashValue, (r) => new Lazy<Task<GeoCodeResponse>>(async () =>
            {
                return await CallGeoCodeAPI(req);
            })).Value;
            if (resp != null && resp.Result is GeoCodeResponse)
            {
                reqAdd.GeoCodeResponse = resp.Result;
                reqAdd.GeoCodeResponseID = resp.Id;
                //await _DbContext.CanadianAddresseses.AddAsync(reqAdd.OptimizedAddress);
                await _DbContext.GeoCodeResponses.AddAsync(reqAdd.GeoCodeResponse);
                _DbContext.GeoCodeRequests.Update(reqAdd);
                await _DbContext.SaveChangesAsync();

                return reqAdd.GeoCodeResponse;
            }
            else
            {
                return reqAdd.GeoCodeResponse;
            }
        }

        /// <summary>
        /// Initializes dictionary of Provinces with their names and abbreviation numbers if dictionary is empty,
        /// else it return the value as per the key passed
        /// </summary>
        /// <param name="key">Province name or abbreviation</param>
        /// <returns>Unique number for Province, set in the dictionary</returns>
        private int GetProvinceValue(string key)
        {
            //string provs = @"\b(AB|BC|MB|NB|NL|NS|NT|NU|ON|PE|QC|SK|YT|(BRITISH COLUMBIA)|(ALBERTA)|(SASKATCHEWAN)|(MANITOBA)|(QNTARIO)|(QUEBEC)|(NEW BRUNSWICK)|(NOVA SCOTOA)|(PRINCE EDWARD ISLAND)|(NEWFOUNDLAND AND LABRADOR))\b\s*$";

            int value = 0;

            if (_provinces == null || _provinces.Count <= 0)
            {
                _provinces = new Dictionary<string, int>();
                _provinces.Add("AB", 48);
                _provinces.Add("BC", 59);
                _provinces.Add("MB", 46);
                _provinces.Add("NB", 13);
                _provinces.Add("NL", 10);
                _provinces.Add("NS", 12);
                _provinces.Add("NT", 61);
                _provinces.Add("NU", 62);
                _provinces.Add("ON", 35);
                _provinces.Add("PE", 11);
                _provinces.Add("QC", 24);
                _provinces.Add("SK", 47);
                _provinces.Add("YT", 60);
                _provinces.Add("BRITISH COLUMBIA", 59);
                _provinces.Add("ALBERTA", 48);
                _provinces.Add("SASKATCHEWAN", 47);
                _provinces.Add("MANITOBA", 46);
                _provinces.Add("QNTARIO", 35);
                _provinces.Add("QUEBEC", 24);
                _provinces.Add("NEW BRUNSWICK", 13);
                _provinces.Add("NOVA SCOTOA", 12);
                _provinces.Add("PRINCE EDWARD ISLAND", 11);
                _provinces.Add("NEWFOUNDLAND AND LABRADOR", 10);
                _provinces.Add("Yukon", 60);
                _provinces.Add("Northwest Territories", 61);
                _provinces.Add("Nunavut", 62);
            }
            if (!string.IsNullOrEmpty(key))
            {
                value = _provinces.First(x => x.Key == key).Value;
            }
            return value;
        }


        /// <summary>
        /// Checking basic validity of the query
        /// </summary>
        /// <param name="address">Query</param>
        /// <returns>Whether the input is valid or not </returns>
        /// <exception cref="BadRequestException"></exception>
        private bool IsValid(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new BadRequestException("Input address is empty!");
            }
            else if (address.All(char.IsDigit))
            {
                return false;
            }
            return true;
        }


        /// <summary>
        /// Transforms each address into CanadianAddress object
        /// and check all GeoCodeRequest and it's corresponding CanadianAddresses whether the same is present in DB,   
        /// if we got the one then will be using it's GeoCodeResponse or will Create GeoCodeRequest with CanadianAddress object and return the list.
        /// </summary>
        /// <param name="userRequestEachAddresses">List of addresses were part of user request</param>
        /// <returns>List of UserRequestEachAddress objects, with GeoCodeRequest which contains GeoCodeResponse (contains Response from Nominatim API)</returns>
        private async Task<List<UserRequestEachAddress>> NormalizeRawAddress( List<UserRequestEachAddress> userRequestEachAddresses)
        {
            //List<GeoCodeRequest> existingGCRList = new List<GeoCodeRequest>();
            List<GeoCodeRequest> nonexistingGCRList = new List<GeoCodeRequest>();
            List<CanadianAddress> caResult = new List<CanadianAddress>();
            
           
            foreach (var item in userRequestEachAddresses)
            {
                if (item != null && item.GeoCodeRequestID == null)
                {
                    GeoCodeRequest gcr = new GeoCodeRequest();

                    #region "Spliting Each address to fit into Canadian address, to be used to call External API"
                    CanadianAddress ca = TransformToCanadianAddress(item);
                    #endregion

                    try
                    {
                        // Once the transformation is done,
                        // need to check the same in DB, whether incoming request's CA is already present DB
                        // if present in DB then we can use the existing GeoCodingRequest. 
                        var existsgcr = await _DbContext.GeoCodeRequests
                            .Where(e => e.CanadianAddressID > 0)
                            .Join(_DbContext.CanadianAddresseses
                            .Where(r => r.HashValue == ca.HashValue),
                            e => e.CanadianAddressID,
                            i => i.ID,
                            (e, i) => e)
                            .ToListAsync();

                        if (existsgcr.Any())
                        {
                            await _DbContext.Entry(existsgcr[0])
                                .Reference(a => a.CanadianAddress)
                                .LoadAsync();

                            await _DbContext.Entry(existsgcr[0])
                                .Reference(a => a.GeoCodeResponse)
                                .LoadAsync();

                            gcr = existsgcr[0];
                        }
                        else
                        { //caResult.Add(ca);
                            gcr.CanadianAddress = ca;
                            gcr.CanadianAddressID = ca.ID;
                            nonexistingGCRList.Add(gcr);
                        }
                        item.GeoCodeRequests = gcr;
                        item.GeoCodeRequestID = gcr.ID;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error occured while fetching GeoCodeRequests fron DB :{ex.Message}, Inner exception:{ex.InnerException}");
                        throw new ServerError(ex.ToString());
                    }
                }
            }// Foreach END

            try
            {
                if (nonexistingGCRList.Count > 0)
                {
                    await _DbContext.GeoCodeRequests.AddRangeAsync(nonexistingGCRList);
                }
                await _DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while saving data to GeoCodeRequests :{ex.Message}, Inner exception:{ex.InnerException}");
                throw new ServerError(ex.ToString());
            }

            return userRequestEachAddresses;
        }


        /// <summary>
        /// This function extracts the parts of the addresses to form the CanadianAddress object.
        /// </summary>
        /// <param name="item">Object of UserRequestEachAddress, with each address</param>
        /// <returns>Object of CanadianAddress</returns>
        private CanadianAddress TransformToCanadianAddress(UserRequestEachAddress item)
        {
            string provs = @"\b\s+(AB|BC|MB|NB|NL|NS|NT|NU|ON|PE|QC|SK|YT|(BRITISH COLUMBIA)|(ALBERTA)|(SASKATCHEWAN)|(MANITOBA)|(QNTARIO)|(QUEBEC)|(NEW BRUNSWICK)|(NOVA SCOTOA)|(PRINCE EDWARD ISLAND)|(NEWFOUNDLAND AND LABRADOR))\b\s*$";
            string types = @"[A-Z]*\s+\b(ST|AVE|RD|BLVD|CRES|DR|WAY|CRT|PL|CH|TRAIL)\b";
            string dirs = @"\b(N|S|E|W|NW|NE|SW|SE)\b";
            string addressStructures = @"(([a-z]*[A-Z]*)\s*=)";
            string[] removeFromAddress = { "Unit", "Apt", "Suite" };

            CanadianAddress ca= new CanadianAddress();


            string result1 = item.EachAddress;

            // Checking for structured words
            result1 = Regex.Replace(result1, addressStructures, "").Trim();
            result1 = result1.ToUpper();
            result1 = result1.Replace("CANADA", "");

            foreach (string s in removeFromAddress)
            {
                result1 = result1.Replace(s.ToUpper(), "");
            }


            // Checking for Postal code
            // Matches standard V3M 0C3 or V3M0C3 format. 
            // Excludes letters D, F, I, O, Q, U (never used by Canada Post) 
            // Excludes W and Z as the first character.
            var postalRx = new Regex(@"^([ABCEGHJKLMNPRSTVXY]\d[ABCEGHJKLMNPRSTVWXYZ])([\s-]?)(\d[ABCEGHJKLMNPRSTVWXYZ]\d)$");
            var postalMatch = postalRx.Match(result1);
            if (postalMatch.Success)
            {
                ca.PostalCode = postalMatch.Value.Trim();
                // Format with standard middle space if it was missing
                if (ca.PostalCode.Length == 6)
                    ca.PostalCode = ca.PostalCode.Insert(3, " ");

                result1 = result1.Replace(postalMatch.Value, "").Trim();
            }

            // Checking for Province name
            if (result1.Length > 0)
            {
                var provinceRx = new Regex(provs);
                var provinceMatch = provinceRx.Match(result1);
                if (provinceMatch.Success)
                {
                    ca.ProvinceName = provinceMatch.Value.Trim();
                    ca.ProvinceNumber = GetProvinceValue(provinceMatch.Value.Trim());
                    result1 = result1.Replace(provinceMatch.Value, "").Trim();
                }
            }

            // Checking for Civic (build or block) number
            if (result1.Length > 0)
            {
                var bldgRx = new Regex(@"([0-9]*)(\s*[-]*)([0-9]+)");
                var bldgMatch = bldgRx.Match(result1);
                if (bldgMatch.Success)
                {
                    var tempList = bldgMatch.Value.Split("-");
                    if (tempList.Length >= 2)
                    {
                        ca.Unit = tempList[0].Trim();
                        ca.BuildingNumber = tempList[1].Trim();
                    }
                    else
                    {
                        ca.BuildingNumber = tempList[0].Trim();
                    }
                    result1 = result1.Replace(bldgMatch.Value.Trim(), "").Trim();
                }
            }


            // Check for street name and direction           
            if (result1.Length > 0)
            {
                var streetRx = new Regex(types);
                var streetMatch = streetRx.Match(result1);
                if (streetMatch.Success)
                {
                    ca.StreetName = streetMatch.Value.Trim();
                    result1 = result1.Replace(streetMatch.Value.Trim(), "");
                }
            }

            // Checking for Direction
            if (result1.Length > 0)
            {
                var directionRx = new Regex(dirs);
                var directionMatch = directionRx.Match(result1);
                if (directionMatch.Success)
                {
                    ca.Direction = directionMatch.Value.Trim();
                    result1 = result1.Replace(directionMatch.Value.Trim(), "").Trim();
                }
            }
            // Checking for City name
            if (result1.Length > 0)
            {
                var cityRx = new Regex(@"[A-Z]+[\s*][A-Z]*");
                var cityMatch = cityRx.Match(result1);
                if (cityMatch.Success)
                {
                    ca.City = cityMatch.Value.Trim();
                }
            }
            ca.Country = "CANADA";

            ca.HashValue = Utility.GetAddressSHA256(ca.JoinAddress());

            return ca;
        }


        /// <summary>
        /// Separate each address from the list of addresses in query
        /// </summary>
        /// <param name="rawAddress"></param>
        /// <returns>List of addresses</returns>
        private List<string> SplitAddress(string rawAddress)
        {
            var str = Regex.Split(rawAddress, @"}\s*,\s*\n*{");
            var result = str
                .Select(r => { return Regex.Replace(r, @"[.,#""{}]", "").Trim(); }).ToList();

            return result;
        }


        /// <summary>
        /// This function forms the URL with structured query and calls Nominatim API
        /// </summary>
        /// <param name="add">Normalize address to pass to the API</param>
        /// <returns>Response from Nominatim API</returns>
        private async Task<GeoCodeResponse> CallGeoCodeAPI(CanadianAddress add)
        {
            GeoCodeResponse result = null;
            string url = string.Empty;

            // As per Nominatim's user policy need to send 1 request per second
            Thread.Sleep(1000);

            // Forms structured sting using address to form a Query part of the URL
            var tempurl = add.GetUrl();

            // Preponing structured address to base URL of Nominatim API
            if (tempurl != string.Empty)
            {
                url = $"{_httpClient.BaseAddress}{tempurl}";
            }

            // Calling the external API
            var response = await _httpClient.GetFromJsonAsync<List<GeoCodeResponse>>(url);

            if (response != null && response.Count > 0)
            {
                if (response[0] != null)
                {
                    result = (GeoCodeResponse)response[0];
                    return result;
                }
            }
            return result;
        }


        /// <summary>
        /// It saves separated address as an object of UserRequestEachAddress type, 
        /// before saving into DB, it checks whether addresses are present or not with the help of their hash values
        /// </summary>
        /// <param name="result">List of seperated address from query</param>
        /// <returns>Created list of UserRequestEachAddress objects for further processing.</returns>
        private async Task<List<UserRequestEachAddress>> AddUserRequestEachAddress(List<string> result)
        {
            List<UserRequestEachAddress> ureaddress = new List<UserRequestEachAddress>();
            List<UserRequestEachAddress> newAddreses;
            try
            {
                // Get the unique addresses from query
                var uniqueAddress = result
                .Distinct().ToList();

                // Creating corresponding objects of type UserRequestEachAddress, to store each address
                foreach (string address in uniqueAddress)
                {
                    ureaddress.Add(new UserRequestEachAddress()
                    {
                        EachAddress = address,
                        HashValue = Utility.GetAddressSHA256(address)
                    });
                }

                var hashValues = ureaddress
                    .Select(r => r.HashValue)
                    .ToList();

                var existsInDB = await _DbContext.UserRequestEachAddresses
                   .Where(r => hashValues.Contains(r.HashValue))
                   .Select(r => r.HashValue)
                   .ToListAsync();

                // Now creat a list of UserRequestEachAddresses objects, which are not present in DB, so we can use the list to insert into DB
                if (existsInDB.Any())
                {
                    var exists = existsInDB[0];
                    newAddreses = ureaddress
                    .Where(r => !r.HashValue.SequenceEqual(exists))
                    .ToList();
                }
                else
                {
                    newAddreses = ureaddress;
                }
                if (newAddreses != null && newAddreses.Count >0)
                {
                    await _DbContext.UserRequestEachAddresses.AddRangeAsync(newAddreses);
                    await _DbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occured while saving data to UserRequestEachAddresses :{ex.Message}, Inner exception:{ex.InnerException}");
                throw new ServerError(ex.ToString());
            }

            return ureaddress;
        }


        /// <summary>
        /// It saves incoming query into DB if not present, for future use.
        /// It generates HASH value as a unique value for searching purpose.
        /// </summary>
        /// <param name="addressString"></param>
        /// <returns></returns>
        private async Task AddToUserRequest(string addressString)
        {
            byte[] hv = Utility.GetAddressSHA256(addressString);

            var exists = await _DbContext.UserRequests
                .AnyAsync(r => r.HashValue == hv);
            if (!exists)
            {
                UserRequest userReq = new UserRequest();
                userReq.OriginalAddress = addressString;
                userReq.HashValue = hv;

                await _DbContext.UserRequests.AddAsync(userReq);
                await _DbContext.SaveChangesAsync();
            }
        }
        
    }

   
}
