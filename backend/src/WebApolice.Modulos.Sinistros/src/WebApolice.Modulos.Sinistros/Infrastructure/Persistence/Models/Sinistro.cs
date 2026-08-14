using System;
using System.Collections.Generic;

namespace WebApolice.Modulos.Sinistros.src.WebApolice.Modulos.Sinistros.Infrastructure.Persistence.Models;

public partial class Sinistro
{
    public long Id { get; set; }

    public Guid PublicId { get; set; }

    public long PropostaId { get; set; }

    public long? PessoaId { get; set; }

    public long? ClienteId { get; set; }

    public long? ClienteVinculoId { get; set; }

    public long? EstipulanteId { get; set; }

    public long? SeguradoraId { get; set; }

    public short? StatusId { get; set; }

    public string? NumeroSinistro { get; set; }

    public string? SituacaoOriginal { get; set; }

    public DateOnly? DataOcorrencia { get; set; }

    public DateOnly? DataAviso { get; set; }

    public DateOnly? DataEnvioSeguradora { get; set; }

    public DateOnly? DataEncerramento { get; set; }

    public DateTime? DataProtocolo { get; set; }

    public DateTime? DataCarta { get; set; }

    public DateTime? DataRelacaoFamilia { get; set; }

    public DateTime? DataRegulacao { get; set; }

    public decimal? ValorAvisado { get; set; }

    public decimal? ValorImportancia { get; set; }

    public decimal? ValorAuxilioFuneral { get; set; }

    public decimal? ValorCestaBasica { get; set; }

    public decimal? ValorIndenizacao { get; set; }

    public int? TipoPlanoLegadoId { get; set; }

    public string? CpfSinistradoOriginal { get; set; }

    public string? CpfSinistradoLimpo { get; set; }

    public bool CpfSinistradoValido { get; set; }

    public string? Causa { get; set; }

    public string? Observacao { get; set; }

    public int LegadoId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Acompanhamento> Acompanhamentos { get; set; } = new List<Acompanhamento>();

    public virtual ICollection<SinistroBeneficiario> SinistroBeneficiarios { get; set; } = new List<SinistroBeneficiario>();

    public virtual ICollection<SinistroCobertura> SinistroCoberturas { get; set; } = new List<SinistroCobertura>();

    public virtual SinistroStatus? Status { get; set; }
}
