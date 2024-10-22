using CsvHelper.Configuration;
using CsvHelper;
using DB.Data.Models;
using DB.Data.Repositories;
using System.Globalization;

namespace AGRICORE_ABM_object_relational_mapping.Services
{
    /// <summary>
    /// Interface for the Data Importer Service
    /// </summary>
    public interface IDataImporterService
    {
        /// <summary>
        /// Initialise FADN Codes in the database
        /// </summary>
        /// <returns>A boolean indicating if the initialization was successful.</returns>

        Task<bool> InitializeFADNCodes();
    }

    /// <summary>
    /// Service implementation for importing FADN codes into the database.
    /// </summary>
    public class DataImporterService:IDataImporterService
    {
        private readonly IRepository<FADNProduct> _FADNProductRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataImporterService"/> class.
        /// </summary>
        /// <param name="fadnProductRepository">Repository for FADN products.</param>

        public DataImporterService (IRepository<FADNProduct> fadnProductRepository)
        {
            _FADNProductRepository = fadnProductRepository;
        }

        /// <summary>
        /// Initialise FADN Codes in the database.
        /// </summary>
        /// <returns>A boolean indicating if the initialization was successful.</returns>

        public async Task<bool> InitializeFADNCodes()
        {
            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    PrepareHeaderForMatch = args => args.Header.ToLower(),
                };
                using (var reader = new StreamReader("Sources//FADN_crops_codes_dictionary.csv"))
                using (var csv = new CsvReader(reader, config))
                {
                    csv.Context.RegisterClassMap<FADNProductsFromCSVMap>();
                    var records = csv.GetRecords<FADNProduct>().ToList();
                    foreach (var record in records)
                    {
                        var product = await _FADNProductRepository.GetSingleOrDefaultAsync(f => f.FADNIdentifier == record.FADNIdentifier);
                        if(product == null)
                        {
                            await _FADNProductRepository.AddAsync(record);
                        }
                    }
                    
                }
            } catch (Exception ex)
            {
                return false;
            }
            return true;
        }

    }
    /// <summary>
    /// Class map for mapping FADN product data from CSV.
    /// </summary>
    public class FADNProductsFromCSVMap : ClassMap<FADNProduct>
    {
        public FADNProductsFromCSVMap()
        {
            Map(m => m.FADNIdentifier).Name("fadn code");
            Map(m => m.Description).Name("crop description");
            Map(m => m.ProductType).Constant(ProductType.Agricultural);
            Map(m => m.Arable).Name("arable");
        }
    }
}
