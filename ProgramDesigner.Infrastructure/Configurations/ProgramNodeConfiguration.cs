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
    public class ProgramNodeConfiguration : IEntityTypeConfiguration<ProgramNode>
    {
        public void Configure(EntityTypeBuilder<ProgramNode> builder)
        {
            builder.ToTable("Nodes");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.HasOne(x => x.ParentGroup)
                   .WithMany(x => x.Children)
                   .HasForeignKey(x => x.ParentGroupId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Ignore(x => x.Prerequisite);

            builder.HasDiscriminator<string>("Discriminator")
                   .HasValue<Group>("Group")
                   .HasValue<Step>("Step");
        }
    }
}
