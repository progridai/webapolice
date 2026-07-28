using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Seguranca.Migrations
{
    /// <inheritdoc />
    public partial class CargaInicialSeguranca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var timestamp = "'2026-07-23 00:00:00Z'";

            migrationBuilder.Sql($@"
                INSERT INTO seguranca.modulo (public_id, codigo, nome, descricao, icone, ordem, ativo, created_at, updated_at)
                VALUES ('a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11', 'CADASTRO', 'Cadastros', 'Módulo destinado às rotinas cadastrais do WebApólice', 'users', 10, true, {timestamp}, {timestamp});
            ");

            migrationBuilder.Sql($@"
                INSERT INTO seguranca.recurso (public_id, modulo_id, codigo, nome, descricao, rota_frontend, ordem, ativo, created_at, updated_at)
                SELECT 'b1eebc99-9c0b-4ef8-bb6d-6bb9bd380a22', m.id, 'CLIENTES', 'Clientes', 'Cadastro e gerenciamento de clientes', '/clientes', 10, true, {timestamp}, {timestamp}
                FROM seguranca.modulo m
                WHERE m.codigo = 'CADASTRO';
            ");

            migrationBuilder.Sql($@"
                INSERT INTO seguranca.permissao (public_id, recurso_id, codigo, nome, descricao, ativo, created_at, updated_at)
                SELECT 'c1eebc99-9c0b-4ef8-bb6d-6bb9bd380a31', r.id, 'clientes.visualizar', 'Visualizar clientes', 'Permite consultar a listagem e os detalhes dos clientes', true, {timestamp}, {timestamp}
                FROM seguranca.recurso r WHERE r.codigo = 'CLIENTES';

                INSERT INTO seguranca.permissao (public_id, recurso_id, codigo, nome, descricao, ativo, created_at, updated_at)
                SELECT 'c2eebc99-9c0b-4ef8-bb6d-6bb9bd380a32', r.id, 'clientes.inserir', 'Inserir clientes', 'Permite cadastrar novos clientes', true, {timestamp}, {timestamp}
                FROM seguranca.recurso r WHERE r.codigo = 'CLIENTES';

                INSERT INTO seguranca.permissao (public_id, recurso_id, codigo, nome, descricao, ativo, created_at, updated_at)
                SELECT 'c3eebc99-9c0b-4ef8-bb6d-6bb9bd380a33', r.id, 'clientes.alterar', 'Alterar clientes', 'Permite alterar os dados cadastrais dos clientes', true, {timestamp}, {timestamp}
                FROM seguranca.recurso r WHERE r.codigo = 'CLIENTES';

                INSERT INTO seguranca.permissao (public_id, recurso_id, codigo, nome, descricao, ativo, created_at, updated_at)
                SELECT 'c4eebc99-9c0b-4ef8-bb6d-6bb9bd380a34', r.id, 'clientes.inativar', 'Inativar clientes', 'Permite inativar clientes sem exclusão física', true, {timestamp}, {timestamp}
                FROM seguranca.recurso r WHERE r.codigo = 'CLIENTES';

                INSERT INTO seguranca.permissao (public_id, recurso_id, codigo, nome, descricao, ativo, created_at, updated_at)
                SELECT 'c5eebc99-9c0b-4ef8-bb6d-6bb9bd380a35', r.id, 'clientes.reativar', 'Reativar clientes', 'Permite reativar clientes anteriormente inativados', true, {timestamp}, {timestamp}
                FROM seguranca.recurso r WHERE r.codigo = 'CLIENTES';
            ");

            migrationBuilder.Sql($@"
                INSERT INTO seguranca.perfil (public_id, codigo, nome, descricao, perfil_sistema, acesso_total, ativo, created_at, updated_at)
                VALUES 
                ('d1eebc99-9c0b-4ef8-bb6d-6bb9bd380a41', 'ADMINISTRADOR', 'Administrador', 'Perfil interno com acesso total ao WebApólice', true, true, true, {timestamp}, {timestamp}),
                ('d2eebc99-9c0b-4ef8-bb6d-6bb9bd380a42', 'ADMINISTRATIVO', 'Administrativo', 'Perfil destinado às rotinas administrativas da empresa', false, false, true, {timestamp}, {timestamp});
            ");

            migrationBuilder.Sql($@"
                INSERT INTO seguranca.perfil_permissao (perfil_id, permissao_id, atribuido_por_usuario_id, created_at)
                SELECT perfil.id, permissao.id, NULL, {timestamp}
                FROM seguranca.perfil perfil
                CROSS JOIN seguranca.permissao permissao
                WHERE perfil.codigo = 'ADMINISTRATIVO'
                  AND permissao.codigo IN (
                      'clientes.visualizar', 
                      'clientes.inserir', 
                      'clientes.alterar', 
                      'clientes.inativar', 
                      'clientes.reativar'
                  );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM seguranca.perfil_permissao
                WHERE perfil_id IN (SELECT id FROM seguranca.perfil WHERE codigo = 'ADMINISTRATIVO')
                  AND permissao_id IN (SELECT id FROM seguranca.permissao WHERE codigo IN ('clientes.visualizar', 'clientes.inserir', 'clientes.alterar', 'clientes.inativar', 'clientes.reativar'));
            ");

            migrationBuilder.Sql(@"
                DELETE FROM seguranca.perfil WHERE codigo IN ('ADMINISTRADOR', 'ADMINISTRATIVO');
            ");

            migrationBuilder.Sql(@"
                DELETE FROM seguranca.permissao WHERE codigo IN ('clientes.visualizar', 'clientes.inserir', 'clientes.alterar', 'clientes.inativar', 'clientes.reativar');
            ");

            migrationBuilder.Sql(@"
                DELETE FROM seguranca.recurso WHERE codigo = 'CLIENTES';
            ");

            migrationBuilder.Sql(@"
                DELETE FROM seguranca.modulo WHERE codigo = 'CADASTRO';
            ");
        }
    }
}
