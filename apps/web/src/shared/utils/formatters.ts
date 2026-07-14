/**
 * Formata uma data string YYYY-MM-DD para DD/MM/YYYY.
 * Retorna "Não informado" caso a data seja nula ou vazia.
 */
export function formatarDataOuVazio(dataString?: string | null): string {
  if (!dataString) return 'Não informado';
  
  // Trata formato YYYY-MM-DD vindo do DateOnly do C#
  if (/^\d{4}-\d{2}-\d{2}/.test(dataString)) {
    const partes = dataString.substring(0, 10).split('-');
    if (partes.length === 3) {
      return `${partes[2]}/${partes[1]}/${partes[0]}`;
    }
  }

  // Tenta converter se for outro formato ISO
  try {
    const data = new Date(dataString);
    if (!isNaN(data.getTime())) {
      return data.toLocaleDateString('pt-BR');
    }
  } catch {
    // Falha silenciosa para retornar o fallback
  }

  return 'Data inválida';
}
