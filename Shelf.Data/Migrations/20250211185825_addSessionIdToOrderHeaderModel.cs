﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shelf.Data.Migrations
{
    /// <inheritdoc />
    public partial class addSessionIdToOrderHeaderModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "OrderHeaders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "OrderHeaders");
        }
    }
}
