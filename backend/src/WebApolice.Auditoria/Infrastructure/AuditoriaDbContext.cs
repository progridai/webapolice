using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WebApolice.Auditoria.Domain;
using WebApolice.Auditoria.Infrastructure.Mascaramento;

namespace WebApolice.Auditoria.Infrastructure;

public class AuditoriaDbContext : DbContext
{
    public AuditoriaDbContext(DbContextOptions<AuditoriaDbContext> options)
        : base(options)
    {
    }

    public DbSet<RegistroAuditoria> RegistrosAuditoria => Set<RegistroAuditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("auditoria");

        var registro = modelBuilder.Entity<RegistroAuditoria>();
        
        registro.ToTable("registros_auditoria");
        
        registro.HasKey(e => e.Id);
        
        registro.Property(e => e.DataHoraUtc)
            .IsRequired();
            
        registro.Property(e => e.UsuarioIdExterno)
            .HasMaxLength(255);
            
        registro.Property(e => e.UsuarioNome)
            .HasMaxLength(255);
            
        registro.Property(e => e.Acao)
            .IsRequired()
            .HasMaxLength(100);
            
        registro.Property(e => e.Modulo)
            .IsRequired()
            .HasMaxLength(100);
            
        registro.Property(e => e.Recurso)
            .IsRequired()
            .HasMaxLength(200);
            
        registro.Property(e => e.RecursoId)
            .HasMaxLength(255);
            
        registro.Property(e => e.Resultado)
            .IsRequired();
            
        registro.Property(e => e.TraceId)
            .HasMaxLength(255);
            
        registro.Property(e => e.CorrelationId)
            .HasMaxLength(255);
            
        registro.Property(e => e.EnderecoIp)
            .HasMaxLength(45); // Support for IPv6
            
        registro.Property(e => e.Origem)
            .HasMaxLength(255);
            
        registro.Property(e => e.DadosAnteriores)
            .HasColumnType("jsonb");
            
        registro.Property(e => e.DadosPosteriores)
            .HasColumnType("jsonb");
            
        registro.Property(e => e.Metadados)
            .HasColumnType("jsonb");
            
        registro.Property(e => e.MensagemErro)
            .HasMaxLength(2000);

        // Índices
        registro.HasIndex(e => e.DataHoraUtc);
        registro.HasIndex(e => new { e.UsuarioIdExterno, e.DataHoraUtc });
        registro.HasIndex(e => new { e.Modulo, e.Recurso, e.RecursoId });
        registro.HasIndex(e => e.TraceId);
        registro.HasIndex(e => e.CorrelationId);
        registro.HasIndex(e => e.Resultado);

        base.OnModelCreating(modelBuilder);
    }
}
