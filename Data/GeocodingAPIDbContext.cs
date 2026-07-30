using Microsoft.EntityFrameworkCore;
using GeocodingAPI.Models;

namespace GeocodingAPI.Data
{
    public class GeocodingAPIDbContext : DbContext
    {

        public DbSet<UserRequest> UserRequests { get; set; }

        public DbSet<UserRequestEachAddress> UserRequestEachAddresses { get; set; }

        public DbSet<CanadianAddress> CanadianAddresseses { get; set; }

        public DbSet<GeoCodeRequest> GeoCodeRequests { get; set; }


        public DbSet<GeoCodeResponse> GeoCodeResponses { get; set; }

        public GeocodingAPIDbContext(DbContextOptions<GeocodingAPIDbContext> options)
        :base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<GeoCodeResponse>()
                .OwnsOne(u=> u.address);

            modelBuilder.Entity<UserRequestEachAddress>()
                .HasIndex(r => new
                {
                    r.HashValue
                });
        }
        

    }
}
