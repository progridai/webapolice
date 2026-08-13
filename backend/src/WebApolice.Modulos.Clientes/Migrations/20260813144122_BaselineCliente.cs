using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApolice.Modulos.Clientes.Migrations
{
    /// <inheritdoc />
    public partial class BaselineCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // VAZIO. A tabela já existe fisicamente no banco através do dump inicial.
            // O snapshot do EF apenas registrará que chegamos até aqui.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // VAZIO.
        }
    }
}
