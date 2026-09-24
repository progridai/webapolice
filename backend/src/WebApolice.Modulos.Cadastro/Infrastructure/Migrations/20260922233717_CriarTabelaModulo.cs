using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApolice.Modulos.Cadastro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CriarTabelaModulo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "cadastro");

            migrationBuilder.Sql(@"
                CREATE TABLE IF NOT EXISTS cadastro.modulo (
                    id bigint GENERATED ALWAYS AS IDENTITY PRIMARY KEY
                );

                ALTER TABLE cadastro.modulo ADD COLUMN IF NOT EXISTS public_id uuid NOT NULL DEFAULT (gen_random_uuid());
                ALTER TABLE cadastro.modulo ADD COLUMN IF NOT EXISTS nome character varying(150) NOT NULL DEFAULT '';
                ALTER TABLE cadastro.modulo ADD COLUMN IF NOT EXISTS descricao character varying(500);
                ALTER TABLE cadastro.modulo ADD COLUMN IF NOT EXISTS ativo boolean NOT NULL DEFAULT TRUE;
                ALTER TABLE cadastro.modulo ADD COLUMN IF NOT EXISTS created_at timestamp with time zone NOT NULL DEFAULT (now());
                ALTER TABLE cadastro.modulo ADD COLUMN IF NOT EXISTS updated_at timestamp with time zone NOT NULL DEFAULT (now());
                ALTER TABLE cadastro.modulo ADD COLUMN IF NOT EXISTS deleted_at timestamp with time zone;

                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_class c
                        JOIN pg_namespace n ON n.oid = c.relnamespace
                        WHERE c.relname = 'ix_modulo_public_id' AND n.nspname = 'cadastro'
                    ) THEN
                        ALTER TABLE cadastro.modulo ADD CONSTRAINT ix_modulo_public_id UNIQUE (public_id);
                    END IF;
                END $$;

                CREATE INDEX IF NOT EXISTS ix_modulo_ativo ON cadastro.modulo (ativo) WHERE deleted_at IS NULL;
                CREATE INDEX IF NOT EXISTS ix_modulo_nome ON cadastro.modulo (nome);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "modulo",
                schema: "cadastro");
        }
    }
}
