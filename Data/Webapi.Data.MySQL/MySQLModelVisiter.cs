using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Webapi.Data.Infrastructure;

namespace Webapi.Data.MySQL
{
    public class MySQLModelVisiter : IModelVisiter
    {
        public void Visit(ModelBuilder modelBuilder)
        {
            var entityTypes = modelBuilder.Model.GetEntityTypes();
            var anns = modelBuilder.Model.GetAnnotations();
            foreach (var entityType in entityTypes)
            {
                var method = visiterGen.MakeGenericMethod(entityType.ClrType);
                method.Invoke(this, new object[] { modelBuilder });
            }
        }
        static MethodInfo visiterGen = typeof(MySQLModelVisiter).GetMethod(nameof(VisitEntity), BindingFlags.Public | BindingFlags.Instance);
        public void VisitEntity<T>(ModelBuilder modelBuilder) where T : class
        {
            var entityTypeBuilder = modelBuilder.Entity<T>();

            var props = entityTypeBuilder.Metadata.GetProperties();
            foreach (var prop in props)
            {
                if (prop.ClrType == typeof(string))
                {
                    var maxLength = prop.GetMaxLength();
                    if (maxLength.HasValue)
                    {
                        var columnType = maxLength.Value switch
                        {
                            < 2048 => null,
                            >= 2048 => "LongText"
                        };
                        if (columnType != null)
                        {
                            entityTypeBuilder.Property(prop.Name).HasColumnType(columnType);
                        }
                    }
                }
            }
            //var keys = entityTypeBuilder.Metadata.GetKeys();
            //foreach (var key in keys)
            //{
            //    var name = key.GetName();
            //    //entityTypeBuilder.HasNoKey();
            //    //var keyBuilder = entityTypeBuilder.HasKey(name);     
            //    //entityTypeBuilder.HasKey(name)
            //    modelBuilder.HasSequence(name, seqbuilder =>
            //    {
            //        seqbuilder.StartsAt(10000);
            //        seqbuilder.HasMin(10);
            //        seqbuilder.HasMax(60);
            //        seqbuilder.IncrementsBy(17);
            //    });
            //}
        }
    }
}
