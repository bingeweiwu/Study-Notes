﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SulpHurManagementSystem
{
    using System;
    using System.Configuration;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.EntityClient;

    public partial class SCCM_ITL_TLTEntities : DbContext
    {
        public SCCM_ITL_TLTEntities()
            : base("name=SCCM_ITL_TLTEntities")
        {
            var originalConnectionString = ConfigurationManager.ConnectionStrings["SCCM_ITL_TLTEntities"].ConnectionString;
            var entityBuilder = new EntityConnectionStringBuilder(originalConnectionString);
            var factory = DbProviderFactories.GetFactory(entityBuilder.Provider);
            var providerBuilder = factory.CreateConnectionStringBuilder();

            providerBuilder.ConnectionString = entityBuilder.ProviderConnectionString;
            providerBuilder.Add("Password", "7nt>/tUm~l1I\"!P*4wN1p6GW0RV~q0");
            entityBuilder.ProviderConnectionString = providerBuilder.ToString();
            this.Database.Connection.ConnectionString = entityBuilder.ProviderConnectionString;
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<SH_People> SH_People { get; set; }
    }
}