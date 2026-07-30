using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeocodingAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitializeGeoCodeDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CanadianAddresseses",
                columns: table => new
                {
                    ID = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Unit = table.Column<string>(type: "TEXT", nullable: false),
                    BuildingNumber = table.Column<string>(type: "TEXT", nullable: false),
                    StreetName = table.Column<string>(type: "TEXT", nullable: false),
                    Direction = table.Column<string>(type: "TEXT", nullable: false),
                    City = table.Column<string>(type: "TEXT", nullable: false),
                    ProvinceName = table.Column<string>(type: "TEXT", nullable: false),
                    ProvinceNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    PostalCode = table.Column<string>(type: "TEXT", nullable: false),
                    Country = table.Column<string>(type: "TEXT", nullable: false),
                    HashValue = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CanadianAddresseses", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "GeoCodeResponses",
                columns: table => new
                {
                    PlaceId = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Licence = table.Column<string>(type: "TEXT", nullable: false),
                    OSMType = table.Column<string>(type: "TEXT", nullable: false),
                    OSMId = table.Column<long>(type: "INTEGER", nullable: false),
                    Latitude = table.Column<string>(type: "TEXT", nullable: false),
                    Longitude = table.Column<string>(type: "TEXT", nullable: false),
                    ClassName = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    PlaceRank = table.Column<int>(type: "INTEGER", nullable: false),
                    Importance = table.Column<decimal>(type: "TEXT", nullable: false),
                    AddressType = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    address_Road = table.Column<string>(type: "TEXT", nullable: true),
                    address_Neighbourhood = table.Column<string>(type: "TEXT", nullable: true),
                    address_CityDistrict = table.Column<string>(type: "TEXT", nullable: true),
                    address_City = table.Column<string>(type: "TEXT", nullable: true),
                    address_County = table.Column<string>(type: "TEXT", nullable: true),
                    address_State = table.Column<string>(type: "TEXT", nullable: true),
                    address_ISO3166 = table.Column<string>(type: "TEXT", nullable: true),
                    address_Postcode = table.Column<string>(type: "TEXT", nullable: true),
                    address_Country = table.Column<string>(type: "TEXT", nullable: true),
                    address_CountryCode = table.Column<string>(type: "TEXT", nullable: true),
                    BoundingBox = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoCodeResponses", x => x.PlaceId);
                });

            migrationBuilder.CreateTable(
                name: "UserRequests",
                columns: table => new
                {
                    ID = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OriginalAddress = table.Column<string>(type: "TEXT", nullable: false),
                    HashValue = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRequests", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "GeoCodeRequests",
                columns: table => new
                {
                    ID = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserRequestID = table.Column<long>(type: "INTEGER", nullable: false),
                    CanadianAddressID = table.Column<long>(type: "INTEGER", nullable: true),
                    GeoCodeResponseID = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeoCodeRequests", x => x.ID);
                    table.ForeignKey(
                        name: "FK_GeoCodeRequests_CanadianAddresseses_CanadianAddressID",
                        column: x => x.CanadianAddressID,
                        principalTable: "CanadianAddresseses",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_GeoCodeRequests_GeoCodeResponses_GeoCodeResponseID",
                        column: x => x.GeoCodeResponseID,
                        principalTable: "GeoCodeResponses",
                        principalColumn: "PlaceId");
                });

            migrationBuilder.CreateTable(
                name: "UserRequestEachAddresses",
                columns: table => new
                {
                    ID = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EachAddress = table.Column<string>(type: "TEXT", nullable: false),
                    GeoCodeRequestID = table.Column<long>(type: "INTEGER", nullable: true),
                    HashValue = table.Column<byte[]>(type: "BLOB", nullable: false),
                    ErrorInAddress = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRequestEachAddresses", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UserRequestEachAddresses_GeoCodeRequests_GeoCodeRequestID",
                        column: x => x.GeoCodeRequestID,
                        principalTable: "GeoCodeRequests",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_GeoCodeRequests_CanadianAddressID",
                table: "GeoCodeRequests",
                column: "CanadianAddressID");

            migrationBuilder.CreateIndex(
                name: "IX_GeoCodeRequests_GeoCodeResponseID",
                table: "GeoCodeRequests",
                column: "GeoCodeResponseID");

            migrationBuilder.CreateIndex(
                name: "IX_UserRequestEachAddresses_GeoCodeRequestID",
                table: "UserRequestEachAddresses",
                column: "GeoCodeRequestID");

            migrationBuilder.CreateIndex(
                name: "IX_UserRequestEachAddresses_HashValue",
                table: "UserRequestEachAddresses",
                column: "HashValue");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRequestEachAddresses");

            migrationBuilder.DropTable(
                name: "UserRequests");

            migrationBuilder.DropTable(
                name: "GeoCodeRequests");

            migrationBuilder.DropTable(
                name: "CanadianAddresseses");

            migrationBuilder.DropTable(
                name: "GeoCodeResponses");
        }
    }
}
