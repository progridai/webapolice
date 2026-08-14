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

/**
 * Formata um número de telefone ou celular (ex: 51999999999 -> (51) 99999-9999)
 */
export function formatarTelefone(valor?: string | null): string {
  if (!valor) return '';
  const apenasNumeros = valor.replace(/\D/g, '');
  
  if (apenasNumeros.length === 11) {
    return `(${apenasNumeros.substring(0, 2)}) ${apenasNumeros.substring(2, 7)}-${apenasNumeros.substring(7)}`;
  }
  
  if (apenasNumeros.length === 10) {
    return `(${apenasNumeros.substring(0, 2)}) ${apenasNumeros.substring(2, 6)}-${apenasNumeros.substring(6)}`;
  }
  
  return valor;
}

/**
 * Formata o valor do contato com base no seu tipo (EMAIL, CELULAR, TELEFONE)
 */
export function formatarValorContato(tipo: string, valor: string): string {
  const tipoUpper = tipo.toUpperCase();
  if (tipoUpper === 'CELULAR' || tipoUpper === 'TELEFONE') {
    return formatarTelefone(valor);
  }
  return valor;
}

/**
 * Formata um CEP (ex: 93037570 -> 93037-570)
 */
export function formatarCep(valor?: string | null): string {
  if (!valor) return '';
  const apenasNumeros = valor.replace(/\D/g, '');
  if (apenasNumeros.length === 8) {
    return `${apenasNumeros.substring(0, 5)}-${apenasNumeros.substring(5)}`;
  }
  return valor;
}
