using DB.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection.Emit;
using System.Reflection.Metadata;

namespace DB
{
    /// <summary>
    /// Database context for the Agricore application, derived from Entity Framework's DbContext.
    /// Manages interactions with the PostgreSQL database.
    /// </summary>
    public class AgricoreContext : DbContext
    {
        protected readonly IConfiguration Configuration;

        /// <summary>
        /// Table of farms.
        /// </summary>
        public DbSet<Farm> Farms { get; set; }

        /// <summary>
        /// Table of years.
        /// </summary>
        public DbSet<Year> Years { get; set; }

        /// <summary>
        /// Table of policies.
        /// </summary>
        public DbSet<Policy> Policies { get; set; }

        /// <summary>
        /// Table of populations.
        /// </summary>
        public DbSet<Population> Populations { get; set; }

        /// <summary>
        /// Table of farm year subsidies.
        /// </summary>
        public DbSet<FarmYearSubsidy> FarmYearSubsidies { get; set; }

        /// <summary>
        /// Table of closing values for farm values.
        /// </summary>
        public DbSet<ClosingValFarmValue> ClosingValFarmValues { get; set; }

        /// <summary>
        /// Table of livestock production.
        /// </summary>
        public DbSet<LivestockProduction> LivestockProductions { get; set; }

        /// <summary>
        /// Table of agricultural production.
        /// </summary>
        public DbSet<AgriculturalProduction> AgriculturalProductions { get; set; }

        /// <summary>
        /// Table of FADN products.
        /// </summary>
        public DbSet<FADNProduct> FADNProducts { get; set; }

        /// <summary>
        /// Table of product groups.
        /// </summary>
        public DbSet<ProductGroup> ProductGroups { get; set; }

        /// <summary>
        /// Table of holder farm year data.
        /// </summary>
        public DbSet<HolderFarmYearData> HolderFarmYearData { get; set; }

        /// <summary>
        /// Table of land transactions.
        /// </summary>
        public DbSet<LandTransaction> LandTransactions { get; set; }

        /// <summary>
        /// Table of agro management decisions.
        /// </summary>
        public DbSet<AgroManagementDecision> AgroManagementDecisions { get; set; }

        /// <summary>
        /// Table of greening farm year data.
        /// </summary>
        public DbSet<GreeningFarmYearData> GreeningFarmYearData { get; set; }

        /// <summary>
        /// Table of land rents.
        /// </summary>
        public DbSet<LandRent> LandRents { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgricoreContext"/> class.
        /// </summary>
        /// <param name="options">The options to be used by a DbContext.</param>
        /// <param name="configuration">The application configuration.</param>
        public AgricoreContext(DbContextOptions<AgricoreContext> options, IConfiguration configuration) : base(options)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Configures the model that was discovered by convention from the entity types exposed in DbSet properties on your derived context.
        /// </summary>
        /// <param name="builder">Model builder to be configured.</param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // This is specific for TPC entities
            // We use 2 sequences to generate values that can never overlap
            // we also configure so the Id property can never be written in these entities
            // to avoid collisions.

            /*        builder.Entity<FADNGroup>()
                        .HasMany(s => s.FADNProducts)
                        .WithOne(s => s.FADNGroup)
                        .HasForeignKey(e => e.FADNGroupId)
                        .IsRequired(false)
                        .HasPrincipalKey(e => e.Id);*/
        }

        /// <summary>
        /// Configures the database context with the connection string.
        /// </summary>
        /// <param name="options">Options builder for DbContext.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // Connect to PostgreSQL with connection string from web app settings
            var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTIONSTRING") ?? "";
            // Console.WriteLine("Using Connection String inside DBContextFactory: " + connectionString);
            // options.UseNpgsql(connectionString).LogTo(Console.WriteLine).EnableDetailedErrors().EnableSensitiveDataLogging();
            options.UseNpgsql(connectionString);
        }
    }

    /// <summary>
    /// Design-time factory for creating instances of the <see cref="AgricoreContext"/>.
    /// </summary>
    public class AgricoreContextFactory : IDesignTimeDbContextFactory<AgricoreContext>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="AgricoreContext"/> class.
        /// </summary>
        /// <param name="args">Arguments for creating the context.</param>
        /// <returns>An instance of <see cref="AgricoreContext"/>.</returns>
        public AgricoreContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AgricoreContext>();
            // This is hardcoded as I didn't find a way to set the property in a config file for a library project
            var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTIONSTRING") ?? "";
            // Console.WriteLine("Using Connection String inside DBContextFactory: " + connectionString);
            // optionsBuilder.UseNpgsql(connectionString).LogTo(Console.WriteLine).EnableDetailedErrors().EnableSensitiveDataLogging();
            optionsBuilder.UseNpgsql(connectionString);

            return new AgricoreContext(optionsBuilder.Options, null);
        }
    }


}