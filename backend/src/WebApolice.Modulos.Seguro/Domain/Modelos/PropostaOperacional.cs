namespace WebApolice.Modulos.Seguro.Domain.Modelos;

public partial class Propostum
{
    public DateOnly? DataCancelamentoVigencia { get; set; }
    public DateTime? CancelamentoSolicitadoEm { get; set; }
    public DateTime? DescartadaEm { get; set; }
    public bool DescartadaSemVigencia(string? codigo) => !string.IsNullOrWhiteSpace(codigo) &&
        DescartadaEm != null && ConfirmadaEm == null && CanceladaEm == null && DataCancelamentoVigencia == null && CancelamentoSolicitadoEm == null &&
        DataInicioVigencia == null && DataFimVigencia == null && LegadoId == null && !Vigente && SubstituidaEm == null &&
        DeletedAt == null && VisivelOperacional && Status.Codigo == codigo && Status.VisivelOperacional &&
        Status.Finalizador && !Status.PermiteMovimentacao;
    public bool TentativaEncerradaSemVigencia(string? cancelada, string? descartada) =>
        CanceladaSemVigencia(cancelada) || DescartadaSemVigencia(descartada);
    public DateTime? CanceladaEm { get; set; }
    public bool CanceladaSemVigencia(string? codigo) => !string.IsNullOrWhiteSpace(codigo) &&
        DescartadaEm == null && DataCancelamentoVigencia == null && CancelamentoSolicitadoEm == null && CanceladaEm != null && ConfirmadaEm != null && DataInicioVigencia != null &&
        LegadoId == null && !Vigente && SubstituidaEm == null && DataFimVigencia == null &&
        DeletedAt == null && VisivelOperacional && Status.Codigo == codigo &&
        Status.VisivelOperacional && Status.Finalizador && !Status.PermiteMovimentacao;
    public string? MotivoPendenciaVigencia { get; set; }
    public DateTime? PendenciaVigenciaEm { get; set; }
    public DateTime? ConfirmadaEm { get; set; }
    public DateTime? SubstituidaEm { get; set; }
    public DateOnly? DataInicioVigencia { get; set; }
    public DateOnly? DataFimVigencia { get; set; }
    public int RevisaoPreenchimento { get; set; }
    public bool PermitePreenchimento(string? codigoInicial) => !string.IsNullOrWhiteSpace(codigoInicial) &&
        DescartadaEm == null && DataCancelamentoVigencia == null && CancelamentoSolicitadoEm == null && CanceladaEm == null && ConfirmadaEm == null && SubstituidaEm == null && LegadoId == null && ApoliceId != null && ApoliceVidaId != null && DeletedAt == null &&
        VisivelOperacional && !Vigente && Status.Codigo == codigoInicial &&
        !Status.Finalizador && !Status.PermiteMovimentacao;
    public long? ApoliceSubgrupoId { get; set; }
    public long? ApoliceModuloId { get; set; }
    public string? SubgrupoNomeRegistrado { get; set; }
    public string? ModuloNomeRegistrado { get; set; }
    public ApoliceSubgrupoModel? ApoliceSubgrupo { get; set; }
    public ApoliceModuloModel? ApoliceModulo { get; set; }
}

public partial class PropostaItem
{
    public long? ApoliceModuloPlanoId { get; set; }
    public string? PlanoNomeRegistrado { get; set; }
    public bool? PlanoPaga { get; set; }
    public bool? PlanoReajuste { get; set; }
    public ApoliceModuloPlanoModel? ApoliceModuloPlano { get; set; }
}

public partial class PropostaCobertura
{
    public bool PremioAjustado { get; set; }
    public decimal? PremioTitularInicial { get; set; }
    public decimal? PremioConjugeInicial { get; set; }
    public string? JustificativaAjustePremio { get; set; }
    public Guid PublicId { get; set; } = Guid.NewGuid();
    public long? ApoliceModuloCoberturaId { get; set; }
    public string? CoberturaNomeRegistrado { get; set; }
    public string? CoberturaNomeReduzidoRegistrado { get; set; }
    public string? CoberturaBasicaRegistrada { get; set; }
    public bool? CoberturaReajuste { get; set; }
    public ApoliceModuloCoberturaModel? ApoliceModuloCobertura { get; set; }
}
