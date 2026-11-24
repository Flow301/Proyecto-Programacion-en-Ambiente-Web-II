namespace SGMA.Domain.Entities;

// Variable de control del mantenimiento preventivo (enunciado Avance 2, 5.8): la medida de uso real que define cada categoría.
// Catálogo fijo que carga el CatalogSeeder. En flota vehicular solo existe Kilometraje (docs/decisiones-tecnicas.md, sección 1).
public class VariableControl
{
    public int IdVariableControl { get; set; }
    public string Nombre { get; set; } = null!;
    public string Unidad { get; set; } = null!;

    public virtual ICollection<CategoriaActivo> Categorias { get; set; } = new List<CategoriaActivo>();
}
