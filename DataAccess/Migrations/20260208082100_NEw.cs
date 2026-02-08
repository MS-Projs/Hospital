using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class NEw : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_doctor_certificates_CertificateTypes_category",
                schema: "public",
                table: "doctor_certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_patient_documents_DocumentCategories_document_category_id",
                schema: "public",
                table: "patient_documents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DocumentCategories",
                table: "DocumentCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CertificateTypes",
                table: "CertificateTypes");

            migrationBuilder.RenameTable(
                name: "DocumentCategories",
                newName: "document_category",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "CertificateTypes",
                newName: "certificate_types",
                newSchema: "public");

            migrationBuilder.AddPrimaryKey(
                name: "PK_document_category",
                schema: "public",
                table: "document_category",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_certificate_types",
                schema: "public",
                table: "certificate_types",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_doctor_certificates_certificate_types_category",
                schema: "public",
                table: "doctor_certificates",
                column: "category",
                principalSchema: "public",
                principalTable: "certificate_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_patient_documents_document_category_document_category_id",
                schema: "public",
                table: "patient_documents",
                column: "document_category_id",
                principalSchema: "public",
                principalTable: "document_category",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_doctor_certificates_certificate_types_category",
                schema: "public",
                table: "doctor_certificates");

            migrationBuilder.DropForeignKey(
                name: "FK_patient_documents_document_category_document_category_id",
                schema: "public",
                table: "patient_documents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_document_category",
                schema: "public",
                table: "document_category");

            migrationBuilder.DropPrimaryKey(
                name: "PK_certificate_types",
                schema: "public",
                table: "certificate_types");

            migrationBuilder.RenameTable(
                name: "document_category",
                schema: "public",
                newName: "DocumentCategories");

            migrationBuilder.RenameTable(
                name: "certificate_types",
                schema: "public",
                newName: "CertificateTypes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DocumentCategories",
                table: "DocumentCategories",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CertificateTypes",
                table: "CertificateTypes",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_doctor_certificates_CertificateTypes_category",
                schema: "public",
                table: "doctor_certificates",
                column: "category",
                principalTable: "CertificateTypes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_patient_documents_DocumentCategories_document_category_id",
                schema: "public",
                table: "patient_documents",
                column: "document_category_id",
                principalTable: "DocumentCategories",
                principalColumn: "id");
        }
    }
}
