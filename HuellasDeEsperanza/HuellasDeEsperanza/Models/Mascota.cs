namespace HuellasDeEsperanza.Models
{
    public class Mascota
    {
        public enum Tamanio
        {
            Pequeño,
            Mediano,
            Grande
        }
        public int Id { get; set; }

        public string Nombre { get; set; }

        public int Edad { get; set; }
    }
}
