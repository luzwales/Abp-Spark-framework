using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace Abp.Spark.Customers;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("AppCustomers");
        builder.ConfigureByConvention();

        builder.Property(x => x.Code).IsRequired().HasMaxLength(64);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(128);
        builder.Property(x => x.ShortName).IsRequired().HasMaxLength(64);
        builder.Property(x => x.TaxNumber).HasMaxLength(64);
        builder.Property(x => x.IndustryType).HasMaxLength(64);
        builder.Property(x => x.ContactPerson).IsRequired().HasMaxLength(64);
        builder.Property(x => x.ContactPhone).IsRequired().HasMaxLength(32);
        builder.Property(x => x.ContactEmail).HasMaxLength(128);
        builder.Property(x => x.CreditLevel).HasMaxLength(16);
        builder.Property(x => x.SettlementMethod).HasMaxLength(64);
        builder.Property(x => x.Remark).HasMaxLength(512);

        builder.HasIndex(x => x.Code);
        builder.HasIndex(x => x.Name);
    }
}
