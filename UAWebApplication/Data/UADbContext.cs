using System;
using System.Collections.Generic;
using UAWebApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace UAWebApplication.Data;

public partial class UADbContext : DbContext
{
    public UADbContext()
    {
    }

    public UADbContext(DbContextOptions<UADbContext> options) : base(options)
    {
    }

    public virtual DbSet<AccountContactTable> AccountContactTables { get; set; }

    public virtual DbSet<AccountTable> AccountTables { get; set; }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserRole> AspNetUserRoles { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<BankTable> BankTables { get; set; }

    public virtual DbSet<DestinationTable> DestinationTables { get; set; }

    public virtual DbSet<IsclosingTable> IsclosingTables { get; set; }

    public virtual DbSet<JournalTable> JournalTables { get; set; }

    public virtual DbSet<LoginTable> LoginTables { get; set; }

    public virtual DbSet<LorryBillPrintTable> LorryBillPrintTables { get; set; }

    public virtual DbSet<LorryBillTable> LorryBillTables { get; set; }

    public virtual DbSet<LorryImagesTable> LorryImagesTables { get; set; }

    public virtual DbSet<LorryTable> LorryTables { get; set; }

    public virtual DbSet<PartyBillTable> PartyBillTables { get; set; }

    public virtual DbSet<ProductTable> ProductTables { get; set; }

    public virtual DbSet<ProvincesTable> ProvincesTables { get; set; }

    public virtual DbSet<PsosummaryTable> PsosummaryTables { get; set; }

    public virtual DbSet<ResourceTable> ResourceTables { get; set; }

    public virtual DbSet<SalesTaxInvoicesTable> SalesTaxInvoicesTables { get; set; }

    public virtual DbSet<SetupForBackupTable> SetupForBackupTables { get; set; }

    public virtual DbSet<ShippingTable> ShippingTables { get; set; }

    public virtual DbSet<TripTable> TripTables { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountContactTable>(entity =>
        {
            entity.HasKey(e => e.AccountContactId).HasName("PK__AccountC__78F37F52CF3F8F66");

            entity.ToTable("AccountContactTable");

            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.ContactNo).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.Account).WithMany(p => p.AccountContactTables)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("FK-AccountContactTable_AccountTable");
        });

        modelBuilder.Entity<AccountTable>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__tmp_ms_x__349DA586A01576D6");

            entity.ToTable("AccountTable");

            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.AccountType).HasMaxLength(50);
            entity.Property(e => e.GroupType).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(250);
            entity.Property(e => e.TitleUrdu).HasMaxLength(200);
            entity.Property(e => e.TransactionDate).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.AccountTables)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK-AccountTable_LoginTable");
        });

        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);

            entity.HasOne(d => d.Resource).WithMany(p => p.AspNetRoles)
                .HasForeignKey(d => d.ResourceId)
                .HasConstraintName("FK_AspNetRoles_ResourceTable");
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.DisplayName).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.UserName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.ProviderKey).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserRole>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetUserRoles_RoleId");

            entity.Property(e => e.UserId).HasMaxLength(450);

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetUserRoles).HasForeignKey(d => d.RoleId);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserRoles).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.Name).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<BankTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tmp_ms_x__3214EC279F77038D");

            entity.ToTable("BankTable");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AccountNo)
                .HasMaxLength(100)
                .HasColumnName("AccountNO");
            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.BankCode).HasMaxLength(100);
            entity.Property(e => e.BankName).HasMaxLength(100);

            entity.HasOne(d => d.Account).WithMany(p => p.BankTables)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("FK-BankTable_AccountTable");
        });

        modelBuilder.Entity<DestinationTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Destinat__3214EC0754A1A96F");

            entity.ToTable("DestinationTable");

            entity.Property(e => e.DestinationCode).HasMaxLength(50);
            entity.Property(e => e.FreightRatePerTon).HasColumnType("money");
            entity.Property(e => e.Title).HasMaxLength(50);
            entity.Property(e => e.TitleUrdu).HasMaxLength(50);

            entity.HasOne(d => d.Party).WithMany(p => p.DestinationTables)
                .HasForeignKey(d => d.PartyId)
                .HasConstraintName("FK_DestinationTable_AccountTable");
        });

        modelBuilder.Entity<IsclosingTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_IClosingTable");

            entity.ToTable("ISClosingTable");

            entity.Property(e => e.Date1).HasColumnType("datetime");
            entity.Property(e => e.Income).HasColumnType("money");
            entity.Property(e => e.TotalExpense).HasColumnType("money");
            entity.Property(e => e.TotalRevenue).HasColumnType("money");
        });

        modelBuilder.Entity<JournalTable>(entity =>
        {
            entity.ToTable("JournalTable");

            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.ChequeNo)
                .HasMaxLength(50)
                .HasColumnName("ChequeNO");
            entity.Property(e => e.Credit).HasColumnType("money");
            entity.Property(e => e.Debit).HasColumnType("money");
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.EntryDate).HasColumnType("datetime");
            entity.Property(e => e.EntryType).HasMaxLength(50);
            entity.Property(e => e.Lorry).HasMaxLength(50);
            entity.Property(e => e.LorryBillNo).HasColumnName("LorryBillNO");
            entity.Property(e => e.PartyBillId).HasColumnName("PartyBillID");
            entity.Property(e => e.Quanitity).HasColumnType("money");
            entity.Property(e => e.ReceiverName).HasMaxLength(150);
            entity.Property(e => e.SummaryId).HasColumnName("SummaryID");
            entity.Property(e => e.TransId).HasColumnName("TransID");
            entity.Property(e => e.TransactionDate).HasColumnType("datetime");
            entity.Property(e => e.TripId).HasColumnName("TripID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Account).WithMany(p => p.JournalTables)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("FK-JournalTable_AccountTable");

            entity.HasOne(d => d.Close).WithMany(p => p.JournalTables)
                .HasForeignKey(d => d.CloseId)
                .HasConstraintName("FK_JournalTable_IClosingTable");

            entity.HasOne(d => d.LorryBillNoNavigation).WithMany(p => p.JournalTables)
                .HasForeignKey(d => d.LorryBillNo)
                .HasConstraintName("FK_JournalTable_LorryBillTable");

            entity.HasOne(d => d.PartyBill).WithMany(p => p.JournalTables)
                .HasForeignKey(d => d.PartyBillId)
                .HasConstraintName("FK_JournalTable_PartyBillTable");

            entity.HasOne(d => d.Summary).WithMany(p => p.JournalTables)
                .HasForeignKey(d => d.SummaryId)
                .HasConstraintName("FK-JournalTable_PSOSummaryTable");

            entity.HasOne(d => d.Trip).WithMany(p => p.JournalTables)
                .HasForeignKey(d => d.TripId)
                .HasConstraintName("FK_JournalTable_TripTable");

            entity.HasOne(d => d.User).WithMany(p => p.JournalTables)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_JournalTable_LoginTable");
        });

        modelBuilder.Entity<LoginTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LoginTab__3214EC07EEC99FA4");

            entity.ToTable("LoginTable");

            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.UserId)
                .HasMaxLength(100)
                .HasColumnName("UserID");
            entity.Property(e => e.UserName).HasMaxLength(100);
        });

        modelBuilder.Entity<LorryBillPrintTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LorryBil__3214EC07A88DD06C");

            entity.ToTable("LorryBillPrintTable");

            entity.HasOne(d => d.LorryBillNavigation).WithMany(p => p.LorryBillPrintTables)
                .HasForeignKey(d => d.LorryBill)
                .HasConstraintName("FK_LorryBillPrintTable_LorryBillTable");
        });

        modelBuilder.Entity<LorryBillTable>(entity =>
        {
            entity.HasKey(e => e.BillNo).HasName("PK__LorryBil__11F287F998861E3C");

            entity.ToTable("LorryBillTable");

            entity.Property(e => e.BillNo).HasColumnName("BillNO");
            entity.Property(e => e.BillCharges).HasColumnType("money");
            entity.Property(e => e.BillDate).HasColumnType("datetime");
            entity.Property(e => e.BillDateString).HasMaxLength(50);
            entity.Property(e => e.OwnerName).HasMaxLength(50);
            entity.Property(e => e.TransactionDate).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.LorryNavigation).WithMany(p => p.LorryBillTables)
                .HasForeignKey(d => d.Lorry)
                .HasConstraintName("FK-LorryBillTable_AccountTable");

            entity.HasOne(d => d.User).WithMany(p => p.LorryBillTables)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK-LorryBillTable_LoginTable");
        });

        modelBuilder.Entity<LorryImagesTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LorryIma__3214EC07F24714E7");

            entity.ToTable("LorryImagesTable");

            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.Date1).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.Image1).HasColumnType("image");
            entity.Property(e => e.TripId).HasColumnName("TripID");

            entity.HasOne(d => d.Account).WithMany(p => p.LorryImagesTables)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("FK-LorryImagesTable_AccountTable");

            entity.HasOne(d => d.Trip).WithMany(p => p.LorryImagesTables)
                .HasForeignKey(d => d.TripId)
                .HasConstraintName("FK-LorryImagesTable_TripTable");
        });

        modelBuilder.Entity<LorryTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__LorryTab__3214EC0731D37D3C");

            entity.ToTable("LorryTable");

            entity.Property(e => e.AccountId).HasColumnName("AccountID");
            entity.Property(e => e.Capacity).HasMaxLength(50);
            entity.Property(e => e.ChassisNo).HasMaxLength(50);
            entity.Property(e => e.CommissionPercent).HasColumnType("money");
            entity.Property(e => e.DipChartDueDate).HasColumnType("datetime");
            entity.Property(e => e.EngineNo).HasMaxLength(50);
            entity.Property(e => e.Make).HasMaxLength(50);
            entity.Property(e => e.Model).HasMaxLength(50);
            entity.Property(e => e.OwnerName).HasMaxLength(50);
            entity.Property(e => e.OwnerNameInUrdu).HasMaxLength(50);
            entity.Property(e => e.TaxPercent).HasColumnType("money");
            entity.Property(e => e.TokenDueDate).HasColumnType("datetime");
            entity.Property(e => e.TrackerDueDate).HasColumnType("datetime");

            entity.HasOne(d => d.Account).WithMany(p => p.LorryTables)
                .HasForeignKey(d => d.AccountId)
                .HasConstraintName("FK-LorryTable_AccountTable");
        });

        modelBuilder.Entity<PartyBillTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PartyBil__3214EC07F2E65A08");

            entity.ToTable("PartyBillTable");

            entity.Property(e => e.BillDate).HasColumnType("datetime");
            entity.Property(e => e.BillNo)
                .HasMaxLength(50)
                .HasColumnName("BillNO");
            entity.Property(e => e.DestinationCity).HasMaxLength(50);
            entity.Property(e => e.DestinationProvince).HasMaxLength(50);
            entity.Property(e => e.ShippingCity).HasMaxLength(50);
            entity.Property(e => e.ShippingProvince).HasMaxLength(50);
            entity.Property(e => e.SummaryId).HasColumnName("SummaryID");
            entity.Property(e => e.TransactionDate).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Party).WithMany(p => p.PartyBillTables)
                .HasForeignKey(d => d.PartyId)
                .HasConstraintName("FK_PartyBillTable_AccountTable");

            entity.HasOne(d => d.Summary).WithMany(p => p.PartyBillTables)
                .HasForeignKey(d => d.SummaryId)
                .HasConstraintName("FK-PartyBillTable_PSOSummaryTable");

            entity.HasOne(d => d.User).WithMany(p => p.PartyBillTables)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK-PartyBillTable_LoginTable");
        });

        modelBuilder.Entity<ProductTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductT__3214EC272B11B9BD");

            entity.ToTable("ProductTable");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ProductCode).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(50);
            entity.Property(e => e.TitleUrdu).HasMaxLength(50);

            entity.HasOne(d => d.Party).WithMany(p => p.ProductTables)
                .HasForeignKey(d => d.PartyId)
                .HasConstraintName("FK_ProductTable_AccountTable");
        });

        modelBuilder.Entity<ProvincesTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Province__3214EC077B06C03A");

            entity.ToTable("ProvincesTable");

            entity.Property(e => e.InterProvinceSalesTax).HasColumnType("money");
            entity.Property(e => e.IntraProvinceSalesTax).HasColumnType("money");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<PsosummaryTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tmp_ms_x__3214EC276D093F32");

            entity.ToTable("PSOSummaryTable");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BankId).HasColumnName("BankID");
            entity.Property(e => e.CreditAmount).HasColumnType("money");
            entity.Property(e => e.Freight).HasColumnType("money");
            entity.Property(e => e.OnlineAmount).HasColumnType("money");
            entity.Property(e => e.PenaltyAmount).HasColumnType("money");
            entity.Property(e => e.ShortAmount).HasColumnType("money");
            entity.Property(e => e.SummaryDate).HasColumnType("datetime");
            entity.Property(e => e.Tax).HasColumnType("money");

            entity.HasOne(d => d.Bank).WithMany(p => p.PsosummaryTableBanks)
                .HasForeignKey(d => d.BankId)
                .HasConstraintName("FK-PSOSummaryTable_AccountTable");

            entity.HasOne(d => d.Company).WithMany(p => p.PsosummaryTableCompanies)
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("FK-PSOSummaryTable_AccountTable2");
        });

        modelBuilder.Entity<ResourceTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Resource__3214EC077D5B2434");

            entity.ToTable("ResourceTable");

            entity.Property(e => e.Title).HasMaxLength(50);
        });

        modelBuilder.Entity<SalesTaxInvoicesTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SalesTax__3214EC07E8BB193E");

            entity.ToTable("SalesTaxInvoicesTable");

            entity.Property(e => e.InvoiceDate).HasColumnType("datetime");
            entity.Property(e => e.InvoiceNo).HasMaxLength(50);
            entity.Property(e => e.PartyBillId).HasColumnName("PartyBillID");
            entity.Property(e => e.PartyId).HasColumnName("PartyID");
            entity.Property(e => e.SalesTaxPercent).HasColumnType("money");

            entity.HasOne(d => d.DestinationProvince).WithMany(p => p.SalesTaxInvoicesTableDestinationProvinces)
                .HasForeignKey(d => d.DestinationProvinceId)
                .HasConstraintName("FK_SalesTaxInvoicesTable_ProvincesTable2");

            entity.HasOne(d => d.InvoiceProvince).WithMany(p => p.SalesTaxInvoicesTableInvoiceProvinces)
                .HasForeignKey(d => d.InvoiceProvinceId)
                .HasConstraintName("FK_SalesTaxInvoicesTable_ProvincesTable3");

            entity.HasOne(d => d.PartyBill).WithMany(p => p.SalesTaxInvoicesTables)
                .HasForeignKey(d => d.PartyBillId)
                .HasConstraintName("FK_SalesTaxInvoicesTable_PartyBillTable");

            entity.HasOne(d => d.Party).WithMany(p => p.SalesTaxInvoicesTables)
                .HasForeignKey(d => d.PartyId)
                .HasConstraintName("FK_SalesTaxInvoicesTable_AccountTable");

            entity.HasOne(d => d.ShippingProvince).WithMany(p => p.SalesTaxInvoicesTableShippingProvinces)
                .HasForeignKey(d => d.ShippingProvinceId)
                .HasConstraintName("FK_SalesTaxInvoicesTable_ProvincesTable1");
        });

        modelBuilder.Entity<SetupForBackupTable>(entity =>
        {
            entity.ToTable("SetupForBackupTable");

            entity.Property(e => e.ComputerName).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.ServerName).HasMaxLength(50);
            entity.Property(e => e.UserId).HasMaxLength(50);
        });

        modelBuilder.Entity<ShippingTable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Shipping__3214EC07EAFE3AD9");

            entity.ToTable("ShippingTable");

            entity.Property(e => e.ShippingCode).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(50);
            entity.Property(e => e.TitleUrdu).HasMaxLength(50);

            entity.HasOne(d => d.Party).WithMany(p => p.ShippingTables)
                .HasForeignKey(d => d.PartyId)
                .HasConstraintName("FK_ShippingTable_AccountTable");
        });

        modelBuilder.Entity<TripTable>(entity =>
        {
            entity.HasKey(e => e.TripId).HasName("PK__tmp_ms_x__51DC711E013805A4");

            entity.ToTable("TripTable");

            entity.Property(e => e.TripId).HasColumnName("TripID");
            entity.Property(e => e.Commission).HasColumnType("money");
            entity.Property(e => e.CommissionPercent).HasColumnType("money");
            entity.Property(e => e.DestinationId).HasColumnName("DestinationID");
            entity.Property(e => e.EntryDate).HasColumnType("datetime");
            entity.Property(e => e.Freight).HasColumnType("money");
            entity.Property(e => e.InvoiceDate).HasColumnType("datetime");
            entity.Property(e => e.LorryBillNo).HasColumnName("LorryBillNO");
            entity.Property(e => e.PartyBillId).HasColumnName("PartyBillID");
            entity.Property(e => e.ProductId).HasColumnName("ProductID");
            entity.Property(e => e.QtyUnit).HasMaxLength(50);
            entity.Property(e => e.ShippingId).HasColumnName("ShippingID");
            entity.Property(e => e.ShortAmount).HasColumnType("money");
            entity.Property(e => e.ShortRate).HasColumnType("money");
            entity.Property(e => e.SummaryId).HasColumnName("SummaryID");
            entity.Property(e => e.SummaryShort).HasColumnType("money");
            entity.Property(e => e.SummaryShortId).HasColumnName("SummaryShortID");
            entity.Property(e => e.Tax).HasColumnType("money");
            entity.Property(e => e.TaxPercent).HasColumnType("money");
            entity.Property(e => e.TransactionDate).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Destination).WithMany(p => p.TripTables)
                .HasForeignKey(d => d.DestinationId)
                .HasConstraintName("FK_TripTable_DestinationTable");

            entity.HasOne(d => d.LorryNavigation).WithMany(p => p.TripTableLorryNavigations)
                .HasForeignKey(d => d.Lorry)
                .HasConstraintName("FK-TripTable_AccountTable");

            entity.HasOne(d => d.LorryBillNoNavigation).WithMany(p => p.TripTables)
                .HasForeignKey(d => d.LorryBillNo)
                .HasConstraintName("FK_TripTable_LorryBillTable");

            entity.HasOne(d => d.PartyBill).WithMany(p => p.TripTables)
                .HasForeignKey(d => d.PartyBillId)
                .HasConstraintName("FK_TripTable_PartyBillTable");

            entity.HasOne(d => d.Party).WithMany(p => p.TripTableParties)
                .HasForeignKey(d => d.PartyId)
                .HasConstraintName("FK_TripTable_AccountTable2");

            entity.HasOne(d => d.Product).WithMany(p => p.TripTables)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_TripTable_ProductTable");

            entity.HasOne(d => d.Shipping).WithMany(p => p.TripTables)
                .HasForeignKey(d => d.ShippingId)
                .HasConstraintName("FK_TripTable_ShippingTable");

            entity.HasOne(d => d.Summary).WithMany(p => p.TripTableSummaries)
                .HasForeignKey(d => d.SummaryId)
                .HasConstraintName("FK-TripTable_PSOSummaryTable");

            entity.HasOne(d => d.SummaryShortNavigation).WithMany(p => p.TripTableSummaryShortNavigations)
                .HasForeignKey(d => d.SummaryShortId)
                .HasConstraintName("FK-TripTable_PSOSummaryTable_Short");

            entity.HasOne(d => d.User).WithMany(p => p.TripTables)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_TripTable_LoginTable");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
