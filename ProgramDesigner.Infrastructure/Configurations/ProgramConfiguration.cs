using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProgramDesigner.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramDesigner.Infrastructure.Configurations
{
    public class ProgramConfiguration : IEntityTypeConfiguration<ProgramEntity>
    {
        public void Configure(EntityTypeBuilder<ProgramEntity> builder)
        {
            builder.ToTable("Programs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.HasOne(p => p.RootGroup)
                   .WithOne()
                   .HasForeignKey<ProgramEntity>(p => p.RootGroupId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
