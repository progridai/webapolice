import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import {
  Button,
  FormField,
  Input,
  Modal,
  ConfirmDialog,
  Alert,
  Table,
  TableHeader,
  TableBody,
  TableRow,
  TableCell,
  Pagination,
} from '../src/components/ui';

describe('UI Components Unit Tests', () => {
  beforeEach(() => {
    vi.stubGlobal('matchMedia', vi.fn().mockImplementation((query) => ({
      matches: false,
      media: query,
      onchange: null,
      addListener: vi.fn(),
      removeListener: vi.fn(),
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
      dispatchEvent: vi.fn(),
    })));
  });

  // 1. Button Tests
  describe('Button Component', () => {
    it('deve renderizar o texto do botão primário e disparar clique', () => {
      const handleClick = vi.fn();
      render(<Button onClick={handleClick}>Enviar</Button>);

      const btn = screen.getByRole('button', { name: /Enviar/i });
      expect(btn).not.toBeNull();
      expect(btn.className).toContain('btn-primary');
      
      fireEvent.click(btn);
      expect(handleClick).toHaveBeenCalledTimes(1);
    });

    it('deve renderizar botão no estado desabilitado', () => {
      const handleClick = vi.fn();
      render(<Button disabled onClick={handleClick}>Enviar</Button>);

      const btn = screen.getByRole('button', { name: /Enviar/i });
      expect(btn.hasAttribute('disabled')).toBe(true);
      
      fireEvent.click(btn);
      expect(handleClick).not.toHaveBeenCalled();
    });

    it('deve exibir spinner e desativar clique no estado loading', () => {
      const handleClick = vi.fn();
      render(<Button loading onClick={handleClick}>Enviar</Button>);

      const btn = screen.getByRole('button', { name: /Processando/i });
      expect(btn.hasAttribute('disabled')).toBe(true);
      expect(screen.getByLabelText('Processando...')).not.toBeNull();
      
      fireEvent.click(btn);
      expect(handleClick).not.toHaveBeenCalled();
    });

    it('deve suportar diferentes variantes e tamanhos', () => {
      const { rerender } = render(<Button variant="danger" size="large">Remover</Button>);
      let btn = screen.getByRole('button', { name: /Remover/i });
      expect(btn.className).toContain('btn-danger');
      expect(btn.className).toContain('btn-large');

      rerender(<Button variant="secondary" size="small">Cancelar</Button>);
      btn = screen.getByRole('button', { name: /Cancelar/i });
      expect(btn.className).toContain('btn-secondary');
      expect(btn.className).toContain('btn-small');
    });
  });

  // 2. Input e FormField Tests
  describe('FormField & Input Components', () => {
    it('deve associar o label com o input e renderizar obrigatoriedade', () => {
      render(
        <FormField label="Nome Completo" required hint="Digite sem abreviações.">
          <Input placeholder="Seu nome" />
        </FormField>
      );

      const label = screen.getByText(/Nome Completo/i);
      expect(label.className).toContain('required');

      const input = screen.getByPlaceholderText(/Seu nome/i);
      expect(input).not.toBeNull();
      expect(label.getAttribute('for')).toBe(input.id);
      
      const hint = screen.getByText(/Digite sem abreviações./i);
      expect(input.getAttribute('aria-describedby')).toContain(hint.id);
    });

    it('deve injetar erros e marcar aria-invalid no input', () => {
      render(
        <FormField label="E-mail" error="E-mail inválido.">
          <Input placeholder="Seu e-mail" />
        </FormField>
      );

      const input = screen.getByPlaceholderText(/Seu e-mail/i);
      expect(input.getAttribute('aria-invalid')).toBe('true');
      expect(input.className).toContain('input-erro');
      
      const errorMsg = screen.getByRole('alert');
      expect(errorMsg.textContent).toContain('E-mail inválido.');
      expect(input.getAttribute('aria-describedby')).toContain(errorMsg.id);
    });
  });

  // 3. Modal Tests
  describe('Modal Component', () => {
    it('deve renderizar modal quando aberto, interceptar Escape e retornar foco', () => {
      const handleClose = vi.fn();
      
      // Cria elemento ativo na tela para testar retorno do foco
      const triggerBtn = document.createElement('button');
      triggerBtn.textContent = 'Open';
      document.body.appendChild(triggerBtn);
      triggerBtn.focus();
      expect(document.activeElement).toBe(triggerBtn);

      const { rerender } = render(
        <Modal aberto={true} onClose={handleClose} title="Janela de Teste">
          <p>Conteúdo do Modal</p>
        </Modal>
      );

      expect(screen.getByRole('dialog')).not.toBeNull();
      expect(screen.getByText('Janela de Teste')).not.toBeNull();

      // Fechamento via clique no botão fechar
      fireEvent.click(screen.getByRole('button', { name: /Fechar Modal/i }));
      expect(handleClose).toHaveBeenCalledTimes(1);

      // Simula tecla Escape
      fireEvent.keyDown(window, { key: 'Escape' });
      expect(handleClose).toHaveBeenCalledTimes(2);

      // Desmonta/fecha modal e verifica se o foco voltou ao triggerBtn original
      rerender(
        <Modal aberto={false} onClose={handleClose} title="Janela de Teste">
          <p>Conteúdo do Modal</p>
        </Modal>
      );
      expect(document.activeElement).toBe(triggerBtn);
      
      // Cleanup
      document.body.removeChild(triggerBtn);
    });
  });

  // 4. ConfirmDialog Tests
  describe('ConfirmDialog Component', () => {
    it('deve disparar onConfirm ao clicar em confirmar e onClose ao cancelar', () => {
      const handleConfirm = vi.fn();
      const handleClose = vi.fn();

      render(
        <ConfirmDialog
          aberto={true}
          onClose={handleClose}
          onConfirm={handleConfirm}
          title="Inativar Item"
          description="Tem certeza disso?"
          confirmText="Sim, inativar"
          cancelText="Não"
          variant="danger"
        />
      );

      expect(screen.getByText('Inativar Item')).not.toBeNull();
      expect(screen.getByText('Tem certeza disso?')).not.toBeNull();

      fireEvent.click(screen.getByRole('button', { name: /Sim, inativar/i }));
      expect(handleConfirm).toHaveBeenCalledTimes(1);

      fireEvent.click(screen.getByRole('button', { name: /Não/i }));
      expect(handleClose).toHaveBeenCalledTimes(1);
    });
  });

  // 5. Alert Tests
  describe('Alert Component', () => {
    it('deve renderizar alertas com diferentes roles e acionar fechamento', () => {
      const handleClose = vi.fn();
      const { rerender } = render(
        <Alert variant="error" title="Atenção" onClose={handleClose}>
          Ocorreu um erro
        </Alert>
      );

      // erro usa role="alert"
      expect(screen.getByRole('alert')).not.toBeNull();
      expect(screen.getByText('Atenção')).not.toBeNull();
      expect(screen.getByText('Ocorreu um erro')).not.toBeNull();

      fireEvent.click(screen.getByRole('button', { name: /Fechar alerta/i }));
      expect(handleClose).toHaveBeenCalledTimes(1);

      // info usa role="status"
      rerender(
        <Alert variant="info">
          Informação geral
        </Alert>
      );
      expect(screen.getByRole('status')).not.toBeNull();
    });
  });

  // 6. Table Tests
  describe('Table Component', () => {
    it('deve renderizar estrutura de tabela, cabeçalhos, colunas e linhas selecionadas', () => {
      render(
        <Table>
          <TableHeader>
            <TableRow>
              <TableCell header>Nome</TableCell>
              <TableCell header>E-mail</TableCell>
            </TableRow>
          </TableHeader>
          <TableBody>
            <TableRow selecionado>
              <TableCell>Rodrigo</TableCell>
              <TableCell>rodrigo@example.com</TableCell>
            </TableRow>
            <TableRow>
              <TableCell>Ana</TableCell>
              <TableCell>ana@example.com</TableCell>
            </TableRow>
          </TableBody>
        </Table>
      );

      expect(screen.getByRole('table')).not.toBeNull();
      expect(screen.getByText('Rodrigo')).not.toBeNull();
      expect(screen.getByText('Ana')).not.toBeNull();

      const selectedRow = screen.getByText('Rodrigo').closest('tr');
      expect(selectedRow?.className).toContain('row-selected-component');
    });
  });

  // 7. Pagination Tests
  describe('Pagination Component', () => {
    it('deve renderizar quantidade de páginas corretas e acionar onPageChange', () => {
      const handlePageChange = vi.fn();
      render(
        <Pagination
          currentPage={1}
          totalPages={3}
          onPageChange={handlePageChange}
          totalItems={15}
          pageSize={5}
        />
      );

      const nextPageBtn = screen.getByRole('button', { name: /Ir para a próxima página/i });
      fireEvent.click(nextPageBtn);
      expect(handlePageChange).toHaveBeenCalledWith(2);

      const page3Btn = screen.getByRole('button', { name: /Ir para a página 3/i });
      fireEvent.click(page3Btn);
      expect(handlePageChange).toHaveBeenCalledWith(3);
    });
  });
});
