using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DB.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FADNProducts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FADNIdentifier = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ProductType = table.Column<int>(type: "integer", nullable: false),
                    Arable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FADNProducts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Populations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Populations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Farms",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Lat = table.Column<long>(type: "bigint", nullable: false),
                    Long = table.Column<long>(type: "bigint", nullable: false),
                    Altitude = table.Column<int>(type: "integer", nullable: false),
                    RegionLevel1 = table.Column<string>(type: "text", nullable: false),
                    RegionLevel1Name = table.Column<string>(type: "text", nullable: false),
                    RegionLevel2 = table.Column<string>(type: "text", nullable: false),
                    RegionLevel2Name = table.Column<string>(type: "text", nullable: false),
                    RegionLevel3 = table.Column<long>(type: "bigint", nullable: false),
                    RegionLevel3Name = table.Column<string>(type: "text", nullable: false),
                    FarmCode = table.Column<string>(type: "text", nullable: false),
                    TechnicalEconomicOrientation = table.Column<int>(type: "integer", nullable: false),
                    PopulationId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Farms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Farms_Populations_PopulationId",
                        column: x => x.PopulationId,
                        principalTable: "Populations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Policies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PolicyIdentifier = table.Column<string>(type: "text", nullable: false),
                    IsCoupled = table.Column<bool>(type: "boolean", nullable: false),
                    PopulationId = table.Column<long>(type: "bigint", nullable: false),
                    PolicyDescription = table.Column<string>(type: "text", nullable: false),
                    EconomicCompensation = table.Column<float>(type: "real", nullable: false),
                    ModelLabel = table.Column<string>(type: "text", nullable: true),
                    StartYearNumber = table.Column<int>(type: "integer", nullable: false),
                    EndYearNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Policies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Policies_Populations_PopulationId",
                        column: x => x.PopulationId,
                        principalTable: "Populations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductGroups",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    ProductType = table.Column<int>(type: "integer", nullable: false),
                    OriginalNameDatasource = table.Column<string>(type: "text", nullable: true),
                    ProductsIncludedInOriginalDataset = table.Column<string>(type: "text", nullable: true),
                    Organic = table.Column<int>(type: "integer", nullable: false),
                    ModelSpecificCategories = table.Column<string[]>(type: "text[]", nullable: false),
                    PopulationId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductGroups_Populations_PopulationId",
                        column: x => x.PopulationId,
                        principalTable: "Populations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Years",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    YearNumber = table.Column<long>(type: "bigint", nullable: false),
                    PopulationId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Years", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Years_Populations_PopulationId",
                        column: x => x.PopulationId,
                        principalTable: "Populations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FADNProductRelation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductGroupId = table.Column<long>(type: "bigint", nullable: false),
                    FADNProductId = table.Column<long>(type: "bigint", nullable: false),
                    PopulationId = table.Column<long>(type: "bigint", nullable: false),
                    RepresentativenessOcurrence = table.Column<float>(type: "real", nullable: false),
                    RepresentativenessArea = table.Column<float>(type: "real", nullable: false),
                    RepresentativenessValue = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FADNProductRelation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FADNProductRelation_FADNProducts_FADNProductId",
                        column: x => x.FADNProductId,
                        principalTable: "FADNProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FADNProductRelation_Populations_PopulationId",
                        column: x => x.PopulationId,
                        principalTable: "Populations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FADNProductRelation_ProductGroups_ProductGroupId",
                        column: x => x.ProductGroupId,
                        principalTable: "ProductGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PolicyGroupRelation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductGroupId = table.Column<long>(type: "bigint", nullable: false),
                    PolicyId = table.Column<long>(type: "bigint", nullable: false),
                    PopulationId = table.Column<long>(type: "bigint", nullable: false),
                    EconomicCompensation = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PolicyGroupRelation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PolicyGroupRelation_Policies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "Policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PolicyGroupRelation_Populations_PopulationId",
                        column: x => x.PopulationId,
                        principalTable: "Populations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PolicyGroupRelation_ProductGroups_ProductGroupId",
                        column: x => x.ProductGroupId,
                        principalTable: "ProductGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AgriculturalProductions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductGroupId = table.Column<long>(type: "bigint", nullable: true),
                    YearId = table.Column<long>(type: "bigint", nullable: false),
                    FarmId = table.Column<long>(type: "bigint", nullable: false),
                    OrganicProductionType = table.Column<int>(type: "integer", nullable: false),
                    CultivatedArea = table.Column<float>(type: "real", nullable: true),
                    IrrigatedArea = table.Column<float>(type: "real", nullable: true),
                    CropProduction = table.Column<float>(type: "real", nullable: true),
                    QuantitySold = table.Column<float>(type: "real", nullable: true),
                    QuantityUsed = table.Column<float>(type: "real", nullable: true),
                    ValueSales = table.Column<float>(type: "real", nullable: true),
                    VariableCosts = table.Column<float>(type: "real", nullable: true),
                    LandValue = table.Column<float>(type: "real", nullable: true),
                    SellingPrice = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgriculturalProductions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgriculturalProductions_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AgriculturalProductions_ProductGroups_ProductGroupId",
                        column: x => x.ProductGroupId,
                        principalTable: "ProductGroups",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AgriculturalProductions_Years_YearId",
                        column: x => x.YearId,
                        principalTable: "Years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AgroManagementDecisions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FarmId = table.Column<long>(type: "bigint", nullable: false),
                    YearId = table.Column<long>(type: "bigint", nullable: false),
                    AgriculturalLandArea = table.Column<float>(type: "real", nullable: false),
                    AgriculturalLandValue = table.Column<float>(type: "real", nullable: false),
                    LongAndMediumTermLoans = table.Column<float>(type: "real", nullable: false),
                    TotalCurrentAssets = table.Column<float>(type: "real", nullable: false),
                    AverageLandValue = table.Column<float>(type: "real", nullable: false),
                    TargetedLandAquisitionArea = table.Column<float>(type: "real", nullable: false),
                    TargetedLandAquisitionHectarPrice = table.Column<float>(type: "real", nullable: false),
                    RetireAndHandOver = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgroManagementDecisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgroManagementDecisions_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AgroManagementDecisions_Years_YearId",
                        column: x => x.YearId,
                        principalTable: "Years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClosingValFarmValues",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AgriculturalLandValue = table.Column<float>(type: "real", nullable: false),
                    AgriculturalLandArea = table.Column<float>(type: "real", nullable: false),
                    LandImprovements = table.Column<float>(type: "real", nullable: false),
                    PlantationsValue = table.Column<float>(type: "real", nullable: false),
                    ForestLandValue = table.Column<float>(type: "real", nullable: false),
                    ForestLandArea = table.Column<float>(type: "real", nullable: false),
                    FarmBuildingsValue = table.Column<float>(type: "real", nullable: false),
                    MachineryAndEquipment = table.Column<float>(type: "real", nullable: false),
                    IntangibleAssetsTradable = table.Column<float>(type: "real", nullable: false),
                    IntangibleAssetsNonTradable = table.Column<float>(type: "real", nullable: false),
                    OtherNonCurrentAssets = table.Column<float>(type: "real", nullable: false),
                    LongAndMediumTermLoans = table.Column<float>(type: "real", nullable: false),
                    TotalCurrentAssets = table.Column<float>(type: "real", nullable: false),
                    FarmNetIncome = table.Column<float>(type: "real", nullable: false),
                    GrossFarmIncome = table.Column<float>(type: "real", nullable: false),
                    SubsidiesOnInvestments = table.Column<float>(type: "real", nullable: false),
                    VATBalanceOnInvestments = table.Column<float>(type: "real", nullable: false),
                    TotalOutputCropsAndCropProduction = table.Column<float>(type: "real", nullable: false),
                    TotalOutputLivestockAndLivestockProduction = table.Column<float>(type: "real", nullable: false),
                    OtherOutputs = table.Column<float>(type: "real", nullable: false),
                    TotalIntermediateConsumption = table.Column<float>(type: "real", nullable: false),
                    Taxes = table.Column<float>(type: "real", nullable: false),
                    VatBalanceExcludingInvestments = table.Column<float>(type: "real", nullable: false),
                    FixedAssets = table.Column<float>(type: "real", nullable: false),
                    Depreciation = table.Column<float>(type: "real", nullable: false),
                    TotalExternalFactors = table.Column<float>(type: "real", nullable: false),
                    Machinery = table.Column<float>(type: "real", nullable: false),
                    FarmId = table.Column<long>(type: "bigint", nullable: false),
                    YearId = table.Column<long>(type: "bigint", nullable: false),
                    RentBalance = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClosingValFarmValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClosingValFarmValues_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClosingValFarmValues_Years_YearId",
                        column: x => x.YearId,
                        principalTable: "Years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FarmYearSubsidies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VALUE = table.Column<float>(type: "real", nullable: false),
                    FarmId = table.Column<long>(type: "bigint", nullable: false),
                    YearId = table.Column<long>(type: "bigint", nullable: false),
                    PolicyId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FarmYearSubsidies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FarmYearSubsidies_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FarmYearSubsidies_Policies_PolicyId",
                        column: x => x.PolicyId,
                        principalTable: "Policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FarmYearSubsidies_Years_YearId",
                        column: x => x.YearId,
                        principalTable: "Years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GreeningFarmYearData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FarmId = table.Column<long>(type: "bigint", nullable: false),
                    YearId = table.Column<long>(type: "bigint", nullable: false),
                    GreeningSurface = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GreeningFarmYearData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GreeningFarmYearData_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GreeningFarmYearData_Years_YearId",
                        column: x => x.YearId,
                        principalTable: "Years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HolderFarmYearData",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FarmId = table.Column<long>(type: "bigint", nullable: false),
                    YearId = table.Column<long>(type: "bigint", nullable: false),
                    HolderAge = table.Column<int>(type: "integer", nullable: false),
                    HolderFamilyMembers = table.Column<int>(type: "integer", nullable: false),
                    HolderSuccessorsAge = table.Column<int>(type: "integer", nullable: false),
                    HolderGender = table.Column<int>(type: "integer", nullable: false),
                    HolderSuccessors = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HolderFarmYearData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HolderFarmYearData_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HolderFarmYearData_Years_YearId",
                        column: x => x.YearId,
                        principalTable: "Years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LandRents",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OriginFarmId = table.Column<long>(type: "bigint", nullable: false),
                    DestinationFarmId = table.Column<long>(type: "bigint", nullable: false),
                    YearId = table.Column<long>(type: "bigint", nullable: false),
                    RentValue = table.Column<float>(type: "real", nullable: false),
                    RentArea = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LandRents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LandRents_Farms_DestinationFarmId",
                        column: x => x.DestinationFarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LandRents_Farms_OriginFarmId",
                        column: x => x.OriginFarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LandRents_Years_YearId",
                        column: x => x.YearId,
                        principalTable: "Years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LivestockProductions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductGroupId = table.Column<long>(type: "bigint", nullable: false),
                    FarmId = table.Column<long>(type: "bigint", nullable: false),
                    YearId = table.Column<long>(type: "bigint", nullable: false),
                    NumberOfAnimals = table.Column<float>(type: "real", nullable: false),
                    DairyCows = table.Column<int>(type: "integer", nullable: false),
                    NumberOfAnimalsSold = table.Column<int>(type: "integer", nullable: false),
                    ValueSoldAnimals = table.Column<float>(type: "real", nullable: false),
                    NumberAnimalsForSlaughtering = table.Column<int>(type: "integer", nullable: false),
                    ValueSlaughteredAnimals = table.Column<float>(type: "real", nullable: false),
                    NumberAnimalsRearingBreading = table.Column<float>(type: "real", nullable: false),
                    ValueAnimalsRearingBreading = table.Column<float>(type: "real", nullable: false),
                    MilkTotalProduction = table.Column<float>(type: "real", nullable: false),
                    MilkProductionSold = table.Column<float>(type: "real", nullable: false),
                    MilkTotalSales = table.Column<float>(type: "real", nullable: false),
                    MilkVariableCosts = table.Column<float>(type: "real", nullable: false),
                    WoolTotalProduction = table.Column<float>(type: "real", nullable: false),
                    WoolProductionSold = table.Column<float>(type: "real", nullable: false),
                    EggsTotalSales = table.Column<float>(type: "real", nullable: false),
                    EggsTotalProduction = table.Column<float>(type: "real", nullable: false),
                    EggsProductionSold = table.Column<float>(type: "real", nullable: false),
                    ManureTotalSales = table.Column<float>(type: "real", nullable: false),
                    VariableCosts = table.Column<float>(type: "real", nullable: false),
                    SellingPrice = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LivestockProductions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LivestockProductions_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LivestockProductions_ProductGroups_ProductGroupId",
                        column: x => x.ProductGroupId,
                        principalTable: "ProductGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LivestockProductions_Years_YearId",
                        column: x => x.YearId,
                        principalTable: "Years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SimulationScenario",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PopulationId = table.Column<long>(type: "bigint", nullable: false),
                    IgnoreLP = table.Column<bool>(type: "boolean", nullable: true),
                    IgnoreLMM = table.Column<bool>(type: "boolean", nullable: true),
                    Compress = table.Column<bool>(type: "boolean", nullable: true),
                    YearId = table.Column<long>(type: "bigint", nullable: false),
                    ShortTermModelBranch = table.Column<string>(type: "text", nullable: false),
                    LongTermModelBranch = table.Column<string>(type: "text", nullable: false),
                    Horizon = table.Column<int>(type: "integer", nullable: false),
                    AdditionalPolicies = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimulationScenario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SimulationScenario_Populations_PopulationId",
                        column: x => x.PopulationId,
                        principalTable: "Populations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SimulationScenario_Years_YearId",
                        column: x => x.YearId,
                        principalTable: "Years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SyntheticPopulation",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    PopulationId = table.Column<long>(type: "bigint", nullable: false),
                    YearId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyntheticPopulation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyntheticPopulation_Populations_PopulationId",
                        column: x => x.PopulationId,
                        principalTable: "Populations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SyntheticPopulation_Years_YearId",
                        column: x => x.YearId,
                        principalTable: "Years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LandTransactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProductionId = table.Column<long>(type: "bigint", nullable: false),
                    DestinationFarmId = table.Column<long>(type: "bigint", nullable: false),
                    YearId = table.Column<long>(type: "bigint", nullable: false),
                    Percentage = table.Column<float>(type: "real", nullable: false),
                    SalePrice = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LandTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LandTransactions_AgriculturalProductions_ProductionId",
                        column: x => x.ProductionId,
                        principalTable: "AgriculturalProductions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LandTransactions_Farms_DestinationFarmId",
                        column: x => x.DestinationFarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LandTransactions_Years_YearId",
                        column: x => x.YearId,
                        principalTable: "Years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SimulationRun",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SimulationScenarioId = table.Column<long>(type: "bigint", nullable: false),
                    OverallStatus = table.Column<int>(type: "integer", nullable: false),
                    CurrentStage = table.Column<int>(type: "integer", nullable: false),
                    CurrentYear = table.Column<int>(type: "integer", nullable: false),
                    CurrentSubstage = table.Column<string>(type: "text", nullable: false),
                    CurrentStageProgress = table.Column<int>(type: "integer", nullable: false),
                    CurrentSubStageProgress = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimulationRun", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SimulationRun_SimulationScenario_SimulationScenarioId",
                        column: x => x.SimulationScenarioId,
                        principalTable: "SimulationScenario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LogMessage",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SimulationRunId = table.Column<long>(type: "bigint", nullable: false),
                    TimeStamp = table.Column<long>(type: "bigint", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    LogLevel = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogMessage_SimulationRun_SimulationRunId",
                        column: x => x.SimulationRunId,
                        principalTable: "SimulationRun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgriculturalProductions_FarmId_ProductGroupId_YearId",
                table: "AgriculturalProductions",
                columns: new[] { "FarmId", "ProductGroupId", "YearId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgriculturalProductions_ProductGroupId",
                table: "AgriculturalProductions",
                column: "ProductGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AgriculturalProductions_YearId",
                table: "AgriculturalProductions",
                column: "YearId");

            migrationBuilder.CreateIndex(
                name: "IX_AgroManagementDecisions_FarmId_YearId",
                table: "AgroManagementDecisions",
                columns: new[] { "FarmId", "YearId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgroManagementDecisions_YearId",
                table: "AgroManagementDecisions",
                column: "YearId");

            migrationBuilder.CreateIndex(
                name: "IX_ClosingValFarmValues_FarmId_YearId",
                table: "ClosingValFarmValues",
                columns: new[] { "FarmId", "YearId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClosingValFarmValues_YearId",
                table: "ClosingValFarmValues",
                column: "YearId");

            migrationBuilder.CreateIndex(
                name: "IX_FADNProductRelation_FADNProductId",
                table: "FADNProductRelation",
                column: "FADNProductId");

            migrationBuilder.CreateIndex(
                name: "IX_FADNProductRelation_PopulationId",
                table: "FADNProductRelation",
                column: "PopulationId");

            migrationBuilder.CreateIndex(
                name: "IX_FADNProductRelation_ProductGroupId_FADNProductId_Population~",
                table: "FADNProductRelation",
                columns: new[] { "ProductGroupId", "FADNProductId", "PopulationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FADNProducts_FADNIdentifier",
                table: "FADNProducts",
                column: "FADNIdentifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Farms_FarmCode_PopulationId",
                table: "Farms",
                columns: new[] { "FarmCode", "PopulationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Farms_FarmCode_PopulationId_RegionLevel3",
                table: "Farms",
                columns: new[] { "FarmCode", "PopulationId", "RegionLevel3" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Farms_PopulationId",
                table: "Farms",
                column: "PopulationId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmYearSubsidies_FarmId_YearId_PolicyId",
                table: "FarmYearSubsidies",
                columns: new[] { "FarmId", "YearId", "PolicyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FarmYearSubsidies_PolicyId",
                table: "FarmYearSubsidies",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmYearSubsidies_YearId",
                table: "FarmYearSubsidies",
                column: "YearId");

            migrationBuilder.CreateIndex(
                name: "IX_GreeningFarmYearData_FarmId_YearId",
                table: "GreeningFarmYearData",
                columns: new[] { "FarmId", "YearId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GreeningFarmYearData_YearId",
                table: "GreeningFarmYearData",
                column: "YearId");

            migrationBuilder.CreateIndex(
                name: "IX_HolderFarmYearData_FarmId_YearId",
                table: "HolderFarmYearData",
                columns: new[] { "FarmId", "YearId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HolderFarmYearData_YearId",
                table: "HolderFarmYearData",
                column: "YearId");

            migrationBuilder.CreateIndex(
                name: "IX_LandRents_DestinationFarmId",
                table: "LandRents",
                column: "DestinationFarmId");

            migrationBuilder.CreateIndex(
                name: "IX_LandRents_OriginFarmId_DestinationFarmId_YearId",
                table: "LandRents",
                columns: new[] { "OriginFarmId", "DestinationFarmId", "YearId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LandRents_YearId",
                table: "LandRents",
                column: "YearId");

            migrationBuilder.CreateIndex(
                name: "IX_LandTransactions_DestinationFarmId_ProductionId_YearId",
                table: "LandTransactions",
                columns: new[] { "DestinationFarmId", "ProductionId", "YearId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LandTransactions_ProductionId",
                table: "LandTransactions",
                column: "ProductionId");

            migrationBuilder.CreateIndex(
                name: "IX_LandTransactions_YearId",
                table: "LandTransactions",
                column: "YearId");

            migrationBuilder.CreateIndex(
                name: "IX_LivestockProductions_FarmId_ProductGroupId_YearId",
                table: "LivestockProductions",
                columns: new[] { "FarmId", "ProductGroupId", "YearId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LivestockProductions_ProductGroupId",
                table: "LivestockProductions",
                column: "ProductGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_LivestockProductions_YearId",
                table: "LivestockProductions",
                column: "YearId");

            migrationBuilder.CreateIndex(
                name: "IX_LogMessage_SimulationRunId",
                table: "LogMessage",
                column: "SimulationRunId");

            migrationBuilder.CreateIndex(
                name: "IX_Policies_PopulationId_PolicyIdentifier",
                table: "Policies",
                columns: new[] { "PopulationId", "PolicyIdentifier" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PolicyGroupRelation_PolicyId",
                table: "PolicyGroupRelation",
                column: "PolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyGroupRelation_PopulationId",
                table: "PolicyGroupRelation",
                column: "PopulationId");

            migrationBuilder.CreateIndex(
                name: "IX_PolicyGroupRelation_ProductGroupId_PolicyId_PopulationId",
                table: "PolicyGroupRelation",
                columns: new[] { "ProductGroupId", "PolicyId", "PopulationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductGroups_Name_PopulationId",
                table: "ProductGroups",
                columns: new[] { "Name", "PopulationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductGroups_PopulationId",
                table: "ProductGroups",
                column: "PopulationId");

            migrationBuilder.CreateIndex(
                name: "IX_SimulationRun_SimulationScenarioId",
                table: "SimulationRun",
                column: "SimulationScenarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SimulationScenario_PopulationId",
                table: "SimulationScenario",
                column: "PopulationId");

            migrationBuilder.CreateIndex(
                name: "IX_SimulationScenario_YearId",
                table: "SimulationScenario",
                column: "YearId");

            migrationBuilder.CreateIndex(
                name: "IX_SyntheticPopulation_PopulationId",
                table: "SyntheticPopulation",
                column: "PopulationId");

            migrationBuilder.CreateIndex(
                name: "IX_SyntheticPopulation_YearId",
                table: "SyntheticPopulation",
                column: "YearId");

            migrationBuilder.CreateIndex(
                name: "IX_Years_Id_PopulationId",
                table: "Years",
                columns: new[] { "Id", "PopulationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Years_PopulationId",
                table: "Years",
                column: "PopulationId");

            migrationBuilder.CreateIndex(
                name: "IX_Years_YearNumber_PopulationId",
                table: "Years",
                columns: new[] { "YearNumber", "PopulationId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgroManagementDecisions");

            migrationBuilder.DropTable(
                name: "ClosingValFarmValues");

            migrationBuilder.DropTable(
                name: "FADNProductRelation");

            migrationBuilder.DropTable(
                name: "FarmYearSubsidies");

            migrationBuilder.DropTable(
                name: "GreeningFarmYearData");

            migrationBuilder.DropTable(
                name: "HolderFarmYearData");

            migrationBuilder.DropTable(
                name: "LandRents");

            migrationBuilder.DropTable(
                name: "LandTransactions");

            migrationBuilder.DropTable(
                name: "LivestockProductions");

            migrationBuilder.DropTable(
                name: "LogMessage");

            migrationBuilder.DropTable(
                name: "PolicyGroupRelation");

            migrationBuilder.DropTable(
                name: "SyntheticPopulation");

            migrationBuilder.DropTable(
                name: "FADNProducts");

            migrationBuilder.DropTable(
                name: "AgriculturalProductions");

            migrationBuilder.DropTable(
                name: "SimulationRun");

            migrationBuilder.DropTable(
                name: "Policies");

            migrationBuilder.DropTable(
                name: "Farms");

            migrationBuilder.DropTable(
                name: "ProductGroups");

            migrationBuilder.DropTable(
                name: "SimulationScenario");

            migrationBuilder.DropTable(
                name: "Years");

            migrationBuilder.DropTable(
                name: "Populations");
        }
    }
}
