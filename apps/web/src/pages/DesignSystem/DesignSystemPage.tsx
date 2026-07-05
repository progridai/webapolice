import React, { useState } from 'react';
import {
  Button,
  FormField,
  Input,
  Textarea,
  Select,
  Checkbox,
  Alert,
  Spinner,
  Skeleton,
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardContent,
  Badge,
  EmptyState,
  Modal,
  ConfirmDialog,
  Table,
  TableHeader,
  TableBody,
  TableRow,
  TableCell,
  Pagination,
  SearchIcon,
  SortIcon,
  UsersIcon,
} from '../../components/ui';
import './DesignSystemPage.css';

interface ClienteMock {
  id: number;
  nome: string;
  cpf: string;
  email: string;
  status: 'ativo' | 'inativo';
}

const CLIENTES_INICIAIS: ClienteMock[] = [
  { id: 1, nome: 'Rodrigo Silva de Souza', cpf: '***.482.193-**', email: 'rodrigo.silva@example.com', status: 'ativo' },
  { id: 2, nome: 'Ana Maria Oliveira', cpf: '***.918.423-**', email: 'ana.maria@example.com', status: 'ativo' },
  { id: 3, nome: 'Carlos Souza Araujo', cpf: '***.294.851-**', email: 'carlos.souza@example.com', status: 'inativo' },
  { id: 4, nome: 'Beatriz Santos Pinheiro', cpf: '***.159.357-**', email: 'beatriz.santos@example.com', status: 'ativo' },
  { id: 5, nome: 'Eduardo Pereira Neto', cpf: '***.753.951-**', email: 'eduardo.pereira@example.com', status: 'inativo' },
];

export const DesignSystemPage: React.FC = () => {
  // Controle de estados para simular as interações
  const [modalAberto, setModalAberto] = useState(false);
  const [confirmacaoAberta, setConfirmacaoAberta] = useState(false);
  const [loadingExclusao, setLoadingExclusao] = useState(false);
  const [tabelaCarregando, setTabelaCarregando] = useState(false);
  const [tabelaVazia, setTabelaVazia] = useState(false);
  const [filtroTexto, setFiltroTexto] = useState('');
  const [checkboxSelecionados, setCheckboxSelecionados] = useState<number[]>([]);
  const [paginaAtual, setPaginaAtual] = useState(1);
  const [clienteParaDeletar, setClienteParaDeletar] = useState<number | null>(null);

  // Estados de formulário
  const [nomeInput, setNomeInput] = useState('');
  const [cpfInput, setCpfInput] = useState('');
  const [perfilSelect, setPerfilSelect] = useState('comum');
  const [termoCheckbox, setTermoCheckbox] = useState(false);
  const [comentarioTextarea, setComentarioTextarea] = useState('');
  const [nomeErro, setNomeErro] = useState('');

  const handleSalvarCliente = (e: React.FormEvent) => {
    e.preventDefault();
    if (!nomeInput.trim()) {
      setNomeErro('O nome do cliente é obrigatório.');
    } else {
      setNomeErro('');
      alert('Cadastro de teste validado e enviado!');
    }
  };

  const toggleCheckbox = (id: number) => {
    setCheckboxSelecionados((prev) =>
      prev.includes(id) ? prev.filter((item) => item !== id) : [...prev, id]
    );
  };

  const toggleTodosCheckbox = () => {
    if (checkboxSelecionados.length === CLIENTES_INICIAIS.length) {
      setCheckboxSelecionados([]);
    } else {
      setCheckboxSelecionados(CLIENTES_INICIAIS.map((c) => c.id));
    }
  };

  const handleDeletarClick = (id: number) => {
    setClienteParaDeletar(id);
    setConfirmacaoAberta(true);
  };

  const confirmarExclusao = () => {
    setLoadingExclusao(true);
    setTimeout(() => {
      setLoadingExclusao(false);
      setConfirmacaoAberta(false);
      alert(`Cliente ID ${clienteParaDeletar} inativado com sucesso (Showcase Mock)!`);
      setClienteParaDeletar(null);
    }, 1000);
  };

  const clientesFiltrados = CLIENTES_INICIAIS.filter(
    (c) =>
      c.nome.toLowerCase().includes(filtroTexto.toLowerCase()) ||
      c.email.toLowerCase().includes(filtroTexto.toLowerCase())
  );

  return (
    <div className="design-system-page">
      <div className="main-header">
        <h1>Catálogo de Componentes do Design System</h1>
        <p className="main-subtitle">
          Demonstração visual interativa de todos os componentes UI obrigatórios para o projeto WebApólice.
        </p>
      </div>

      <div className="showcase-grid">
        {/* Seção 1: Identidade e Cores */}
        <Card className="col-span-2">
          <CardHeader>
            <CardTitle>1. Identidade Visual Oferecida e Cores Oficiais</CardTitle>
            <CardDescription>Cores extraídas da marca e estruturadas em tokens semânticos nos temas.</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="colors-grid">
              <div className="color-item">
                <div className="color-swatch swatch-gold" />
                <div className="color-label">
                  <strong>Dourado Oficial</strong>
                  <span>#D4AF37</span>
                  <small>Destaque e Marca</small>
                </div>
              </div>
              <div className="color-item">
                <div className="color-swatch swatch-white" />
                <div className="color-label">
                  <strong>Branco</strong>
                  <span>#FFFFFF</span>
                  <small>Superfícies e Respiros</small>
                </div>
              </div>
              <div className="color-item">
                <div className="color-swatch swatch-dark" />
                <div className="color-label">
                  <strong>Preto Suave</strong>
                  <span>#1A1A1A</span>
                  <small>Textos e Títulos</small>
                </div>
              </div>
              <div className="color-item">
                <div className="color-swatch swatch-gray" />
                <div className="color-label">
                  <strong>Cinza Claro</strong>
                  <span>#F6F6F6</span>
                  <small>Fundos de Aplicativos</small>
                </div>
              </div>
            </div>

            <div className="functional-colors-list mt-4">
              <h4>Cores Funcionais (Uso exclusivo para comunicação de status e feedback)</h4>
              <div className="functional-colors-grid">
                <span className="badge badge-success"><span className="badge-dot" /> Sucesso (Verde)</span>
                <span className="badge badge-error"><span className="badge-dot" /> Erro (Vermelho)</span>
                <span className="badge badge-warning"><span className="badge-dot" /> Alerta (Amarelo)</span>
                <span className="badge badge-info"><span className="badge-dot" /> Informação (Azul)</span>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Seção 2: Botões */}
        <Card>
          <CardHeader>
            <CardTitle>2. Botões (Button)</CardTitle>
            <CardDescription>Variantes, tamanhos e estados interativos.</CardDescription>
          </CardHeader>
          <CardContent className="button-sections">
            <div className="showcase-section">
              <h5>Variantes</h5>
              <div className="button-row">
                <Button variant="primary">Primário</Button>
                <Button variant="secondary">Secundário</Button>
                <Button variant="text">Texto</Button>
                <Button variant="danger">Perigo</Button>
              </div>
            </div>

            <div className="showcase-section mt-3">
              <h5>Tamanhos</h5>
              <div className="button-row flex-align-center">
                <Button size="small">Pequeno</Button>
                <Button size="medium">Médio</Button>
                <Button size="large">Grande</Button>
              </div>
            </div>

            <div className="showcase-section mt-3">
              <h5>Estados Especiais</h5>
              <div className="button-row">
                <Button variant="primary" disabled>Desabilitado</Button>
                <Button variant="primary" loading>Salvando...</Button>
                <Button variant="danger" loading>Processando...</Button>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Seção 3: Feedback e Badges */}
        <Card>
          <CardHeader>
            <CardTitle>3. Badges, Feedback & Carregamento</CardTitle>
            <CardDescription>Badges de status, alertas, spinners e skeletons.</CardDescription>
          </CardHeader>
          <CardContent className="feedback-sections">
            <div className="showcase-section">
              <h5>Badges</h5>
              <div className="badge-row">
                <Badge variant="neutral">Neutro</Badge>
                <Badge variant="brand" dot>Ativo Marca</Badge>
                <Badge variant="success" dot>Sucesso</Badge>
                <Badge variant="warning">Atenção</Badge>
                <Badge variant="error" dot>Erro</Badge>
                <Badge variant="info">Informação</Badge>
              </div>
            </div>

            <div className="showcase-section mt-3">
              <h5>Carregamento e Feedback Visual</h5>
              <div className="loading-row">
                <Spinner size="small" aria-label="Processando pequeno" />
                <Spinner size="medium" aria-label="Processando médio" />
                <Spinner size="large" aria-label="Processando grande" />
              </div>
            </div>

            <div className="showcase-section mt-3">
              <h5>Skeleton (Estruturas de Espaço Reservado)</h5>
              <div className="skeleton-row-wrapper">
                <Skeleton variant="text" width="60%" />
                <Skeleton variant="text" width="80%" />
                <div className="flex-align-center gap-2 mt-1">
                  <Skeleton variant="avatar" />
                  <div style={{ flex: 1 }}>
                    <Skeleton variant="text" width="40%" />
                    <Skeleton variant="text" width="30%" />
                  </div>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Seção 4: Formulários */}
        <Card className="col-span-2">
          <CardHeader>
            <CardTitle>4. Campos de Formulário (FormFields, Inputs & Controls)</CardTitle>
            <CardDescription>Campos estruturados associados por id/aria para conformidade de acessibilidade.</CardDescription>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleSalvarCliente} className="showcase-form-layout">
              <div className="form-grid">
                <FormField
                  label="Nome Completo"
                  required
                  error={nomeErro}
                  hint="Digite o nome completo sem abreviações."
                >
                  <Input
                    placeholder="Ex: João da Silva"
                    value={nomeInput}
                    onChange={(e) => {
                      setNomeInput(e.target.value);
                      if (e.target.value) setNomeErro('');
                    }}
                  />
                </FormField>

                <FormField
                  label="CPF Protegido"
                  hint="Preenchido com a máscara padrão."
                >
                  <Input
                    placeholder="Ex: ***.***.***-**"
                    value={cpfInput}
                    onChange={(e) => setCpfInput(e.target.value)}
                  />
                </FormField>

                <FormField label="Perfil de Acesso">
                  <Select
                    value={perfilSelect}
                    onChange={(e) => setPerfilSelect(e.target.value)}
                    placeholder="Selecione um perfil..."
                  >
                    <option value="comum">Cliente Comum</option>
                    <option value="vip">Cliente VIP</option>
                    <option value="corporativo">Cliente Corporativo</option>
                  </Select>
                </FormField>

                <FormField
                  label="Observações Internas (Textarea)"
                  hint="Limite de 200 caracteres."
                >
                  <Textarea
                    placeholder="Escreva anotações importantes sobre o cliente..."
                    value={comentarioTextarea}
                    onChange={(e) => setComentarioTextarea(e.target.value)}
                    maxLength={200}
                  />
                </FormField>
              </div>

              <div className="checkbox-section mt-3">
                <Checkbox
                  label="Declaro estar ciente e aceitar a Política de Privacidade de Dados (LGPD)"
                  checked={termoCheckbox}
                  onChange={(e) => setTermoCheckbox(e.target.checked)}
                />
              </div>

              <div className="form-actions-showcase mt-3">
                <Button type="submit" variant="primary">
                  Enviar Formulário
                </Button>
                <Button type="reset" variant="secondary" onClick={() => {
                  setNomeInput('');
                  setCpfInput('');
                  setPerfilSelect('comum');
                  setTermoCheckbox(false);
                  setComentarioTextarea('');
                  setNomeErro('');
                }}>
                  Limpar
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>

        {/* Seção 5: Alertas */}
        <Card className="col-span-2">
          <CardHeader>
            <CardTitle>5. Alertas de Estado (Alert)</CardTitle>
            <CardDescription>Mensagens com ícones e cores semânticas funcionais.</CardDescription>
          </CardHeader>
          <CardContent className="alerts-showcase-list">
            <Alert variant="success" title="Cadastro Concluído">
              O cliente foi salvo com êxito e os dados pessoais foram protegidos conforme a LGPD.
            </Alert>
            <Alert variant="error" title="Erro Crítico de Validação">
              Falha ao conectar com o serviço do ERP. As alterações locais não puderam ser gravadas na base.
            </Alert>
            <Alert variant="warning" title="Documentação Pendente">
              O cliente possui pendências cadastrais na Receita Federal. Algumas apólices podem ser recusadas.
            </Alert>
            <Alert variant="info" title="Inativação Temporária">
              A inativação de registros suspende a emissão, mas preserva a rastreabilidade histórica.
            </Alert>
          </CardContent>
        </Card>

        {/* Seção 6: Tabela e Grid de Dados */}
        <Card className="col-span-2">
          <CardHeader className="showcase-table-header">
            <div>
              <CardTitle>6. Tabela & Paginação (Table, Pagination & EmptyState)</CardTitle>
              <CardDescription>Demonstração dos estados de dados, carregamento, ausência de registros e paginação.</CardDescription>
            </div>
            <div className="table-controls-row">
              <Button
                variant="secondary"
                size="small"
                onClick={() => setTabelaCarregando(!tabelaCarregando)}
              >
                Alternar Loading
              </Button>
              <Button
                variant="secondary"
                size="small"
                onClick={() => setTabelaVazia(!tabelaVazia)}
              >
                Alternar Vazio
              </Button>
            </div>
          </CardHeader>
          <CardContent>
            {/* Barra de Filtros */}
            <div className="table-filter-bar-showcase">
              <div className="search-wrapper">
                <SearchIcon />
                <Input
                  type="text"
                  placeholder="Pesquisar..."
                  value={filtroTexto}
                  onChange={(e) => setFiltroTexto(e.target.value)}
                  aria-label="Filtrar dados"
                />
              </div>
              <span className="selected-count">{checkboxSelecionados.length} itens selecionados</span>
            </div>

            {/* Tabela Real ou Skeletons */}
            <div className="table-container-wrapper mt-3">
              {tabelaCarregando ? (
                <div className="table-skeletons" aria-busy="true" aria-label="Carregando tabela">
                  {[1, 2, 3].map((i) => (
                    <div className="table-skeleton-row" key={i}>
                      <Skeleton variant="row" />
                    </div>
                  ))}
                </div>
              ) : tabelaVazia || clientesFiltrados.length === 0 ? (
                <EmptyState
                  title="Nenhum registro encontrado"
                  description="Não encontramos correspondências para a sua pesquisa. Tente ajustar os filtros informados."
                  icon={<UsersIcon size={48} aria-hidden="true" />}
                  action={
                    <Button variant="primary" size="small" onClick={() => { setFiltroTexto(''); setTabelaVazia(false); }}>
                      Limpar Pesquisa
                    </Button>
                  }
                />
              ) : (
                <Table>
                  <TableHeader>
                    <TableRow>
                      <TableCell header style={{ width: '40px' }}>
                        <Checkbox
                          checked={checkboxSelecionados.length === CLIENTES_INICIAIS.length}
                          onChange={toggleTodosCheckbox}
                          aria-label="Selecionar tudo"
                        />
                      </TableCell>
                      <TableCell header>
                        Nome <SortIcon />
                      </TableCell>
                      <TableCell header>CPF</TableCell>
                      <TableCell header>E-mail</TableCell>
                      <TableCell header>Status</TableCell>
                      <TableCell header style={{ width: '100px', textAlign: 'right' }}>Ações</TableCell>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {clientesFiltrados.map((cliente) => (
                      <TableRow
                        key={cliente.id}
                        selecionado={checkboxSelecionados.includes(cliente.id)}
                      >
                        <TableCell>
                          <Checkbox
                            checked={checkboxSelecionados.includes(cliente.id)}
                            onChange={() => toggleCheckbox(cliente.id)}
                            aria-label={`Selecionar ${cliente.nome}`}
                          />
                        </TableCell>
                        <TableCell className="fw-semibold">{cliente.nome}</TableCell>
                        <TableCell className="font-mono">{cliente.cpf}</TableCell>
                        <td>{cliente.email}</td>
                        <TableCell>
                          <Badge
                            variant={cliente.status === 'ativo' ? 'success' : 'neutral'}
                            dot
                          >
                            {cliente.status}
                          </Badge>
                        </TableCell>
                        <TableCell style={{ textAlign: 'right' }}>
                          <Button
                            variant="secondary"
                            size="small"
                            onClick={() => alert(`Editar cliente ID ${cliente.id}`)}
                            aria-label={`Editar ${cliente.nome}`}
                          >
                            ✏️
                          </Button>
                          <Button
                            variant="danger"
                            size="small"
                            onClick={() => handleDeletarClick(cliente.id)}
                            aria-label={`Inativar ${cliente.nome}`}
                            style={{ marginLeft: 'var(--espaco-1)' }}
                          >
                            🗑️
                          </Button>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              )}
            </div>

            {/* Paginação */}
            <div className="table-pagination mt-3">
              <Pagination
                currentPage={paginaAtual}
                totalPages={2}
                onPageChange={(p) => setPaginaAtual(p)}
                totalItems={clientesFiltrados.length}
                pageSize={5}
              />
            </div>
          </CardContent>
        </Card>

        {/* Seção 7: Modais */}
        <Card className="col-span-2">
          <CardHeader>
            <CardTitle>7. Modais e Diálogos (Modal & ConfirmDialog)</CardTitle>
            <CardDescription>Caixas de diálogo com retenção/trap de foco, tecla Escape ativa e acessibilidade.</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="button-row">
              <Button variant="primary" onClick={() => setModalAberto(true)}>
                Abrir Modal Comum
              </Button>
              <Button variant="danger" onClick={() => handleDeletarClick(99)}>
                Abrir Diálogo de Confirmação
              </Button>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Modais renders */}
      <Modal
        aberto={modalAberto}
        onClose={() => setModalAberto(false)}
        title="Modal Exemplo do Design System"
        footer={
          <>
            <Button variant="secondary" onClick={() => setModalAberto(false)}>
              Fechar
            </Button>
            <Button variant="primary" onClick={() => { alert('Confirmado!'); setModalAberto(false); }}>
              Salvar Alterações
            </Button>
          </>
        }
      >
        <p>Este modal foi construído de forma 100% reutilizável.</p>
        <p className="mt-2">Ele bloqueia a rolagem do fundo, intercepta a tecla Escape e devolve o foco ao botão de origem quando fechado.</p>
      </Modal>

      <ConfirmDialog
        aberto={confirmacaoAberta}
        onClose={() => setConfirmacaoAberta(false)}
        onConfirm={confirmarExclusao}
        title="Confirmar Exclusão de Registro"
        description="Você realmente deseja remover/inativar este registro? Essa alteração suspenderá novas emissões associadas a este cadastro."
        variant="danger"
        confirmText="Sim, Inativar"
        cancelText="Não, Cancelar"
        loading={loadingExclusao}
      />
    </div>
  );
};
export default DesignSystemPage;
