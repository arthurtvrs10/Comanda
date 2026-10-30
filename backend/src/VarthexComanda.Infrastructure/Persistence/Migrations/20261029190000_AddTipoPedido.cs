using Microsoft.EntityFrameworkCore.Migrations;
#nullable disable
namespace VarthexComanda.Infrastructure.Persistence.Migrations;
public partial class AddTipoPedido : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(name:"tipo_pedido",table:"comanda",type:"INTEGER",nullable:false,defaultValue:2);
        migrationBuilder.Sql("UPDATE comanda SET tipo_pedido = CASE WHEN numero_mesa IS NOT NULL THEN 0 ELSE 2 END;");
    }
    protected override void Down(MigrationBuilder migrationBuilder)=>migrationBuilder.DropColumn(name:"tipo_pedido",table:"comanda");
}
