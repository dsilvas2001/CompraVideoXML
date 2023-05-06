using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml;
using System.Security.Cryptography.X509Certificates;
using System.Data.SqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace VideoCompra
{
    public partial class Form1 : Form
    {
        ConexionSQL conexionSQL = new ConexionSQL();
        public Form1()
        {
            InitializeComponent();
        }





        private void dtgCompraProducto_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (this.dtgCompraProducto.Columns[e.ColumnIndex].Name == "Eliminarfila")
            {
                int index = e.RowIndex;
                if (index >= 0)
                {
                    dtgCompraProducto.Rows.RemoveAt(index);
                    GestionarResultadoFactura();
                }
            }
        }

        private void btnAñadirProducto_Click(object sender, EventArgs e)
        {
            if (this.verificarExistencia())
            {
                int cantidadproducto = int.Parse(txtcantidadproducto.Text);
                decimal precio_producto = decimal.Parse(txtpreciounitario.Text);
                decimal descuento_producto = decimal.Parse(txtdescuentoproeducto.Text) / 100.0M;
                decimal iva_producto = decimal.Parse(txtivaproducto.Text) / 100.0M;


                decimal subtotal = precio_producto * cantidadproducto;

                decimal descuentoCantidad = subtotal * descuento_producto;

                decimal subtotalConDescuento = subtotal - descuentoCantidad;

                decimal ivaCantidad = subtotalConDescuento * iva_producto;

                decimal total = subtotalConDescuento + ivaCantidad;

                subtotal = Math.Round(subtotal, 2);
                subtotalConDescuento = Math.Round(subtotalConDescuento, 2);
                ivaCantidad = Math.Round(ivaCantidad, 2);
                total = Math.Round(total, 2);


                dtgCompraProducto.Rows.Add(null, txtnombreproducto.Text, txtcantidadproducto.Text, txtpreciounitario.Text,
                    descuentoCantidad.ToString("0.00"), ivaCantidad.ToString("0.00"), txtdescripcion.Text, dtmfechacaducacion.Value.ToShortDateString()
                    , subtotal.ToString("0.00"), total.ToString("0.00"));

                this.GestionarResultadoFactura();
                this.LimpiarDatos();
            }


        }

        public void LimpiarDatos()
        {
            txtnombreproducto.Clear();
            txtcantidadproducto.Clear();
            txtpreciounitario.Clear();
            txtivaproducto.Clear();
            txtdescripcion.Clear();
            txtdescuentoproeducto.Clear();
        }


        public void GestionarResultadoFactura()
        {
            decimal totalproductos = 0.0M;
            int sumcantidadproductos = 0;
            decimal subtotalcompra = 0.0M;
            decimal descuentoCompra = 0.0M;
            decimal ivaproductos = 12M;

            foreach (DataGridViewRow recorrerdata in dtgCompraProducto.Rows)
            {
                totalproductos += decimal.Parse(recorrerdata.Cells["TotalProducto"].Value.ToString());
            }

            subtotalcompra = totalproductos;
            txtsubtotalCompra.Text = subtotalcompra.ToString("0.00");

            descuentoCompra = subtotalcompra * (descuentoCompra / 100.0M);

            subtotalcompra -= descuentoCompra;

            decimal iva = subtotalcompra * (ivaproductos / 100.0M);

            txtresultadoiva.Text = iva.ToString("0.00");

            subtotalcompra += iva;

            decimal total = subtotalcompra;
            txttotalcompra.Text = total.ToString("0.00");
        }

        public bool verificarExistencia()
        {
            foreach (DataGridViewRow recorrerdata in dtgCompraProducto.Rows)
            {
                if (txtnombreproducto.Text == recorrerdata.Cells["NombreProducto"].Value.ToString())
                {
                    MessageBox.Show("Producto ya se encuentra agregado", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
            }
            return true;
        }

        private void btnCompra_Click(object sender, EventArgs e)
        {
            DialogResult respuesta = MessageBox.Show("Deseas realizar esta Compra? Por favor, confirma tu eleccion.", "Advertencia", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (respuesta == DialogResult.Yes)
            {
                try
                {

                    XElement CompraProduct = new XElement("Compra");
                    CompraProduct.Add(new XElement("item",
                        new XElement("iva_compra", decimal.Parse(txtresultadoiva.Text.Replace(".", ","))),
                        new XElement("subtotal", decimal.Parse(txtsubtotalCompra.Text.Replace(".", ","))),
                          new XElement("TotalCompra", decimal.Parse(txttotalcompra.Text.Replace(".", ",")))
                        ));

                    XElement Detalle_Compra = new XElement("Detalle_Compra");

                    foreach (DataGridViewRow recorrerdata in dtgCompraProducto.Rows)
                    {
                        Detalle_Compra.Add(new XElement("item",
                       new XElement("nombre_product", recorrerdata.Cells["NombreProducto"].Value.ToString()),
                       new XElement("Cantidad", recorrerdata.Cells["StockProducto"].Value),
                         new XElement("PrecioUnitario", Convert.ToDecimal(recorrerdata.Cells["PrecioUnitario"].Value.ToString().Replace(".",","))),
                         new XElement("ivaproducto", Convert.ToDecimal(recorrerdata.Cells["IvaProduct"].Value.ToString().Replace(".", ","))),
                         new XElement("Subtotal", Convert.ToDecimal(recorrerdata.Cells["SubtotalProduct"].Value.ToString().Replace(".", ","))),
                         new XElement("total", Convert.ToDecimal(recorrerdata.Cells["TotalProducto"].Value.ToString().Replace(".", ","))),
                         new XElement("descripcion", recorrerdata.Cells["DescripcionProducto"].Value.ToString()),
                         new XElement("fecha_vencimiento", recorrerdata.Cells["VencimientoProducto"].Value.ToString())
                       ));
                    }


                    string consulta = CompraProduct.ToString() + Detalle_Compra.ToString();
                    this.xmlCompra(consulta);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    throw;
                }
            }
        }
        public void xmlCompra(string Detalle)
        {

            try
            {

                SqlCommand cmd = new SqlCommand("sp_compraProducto", conexionSQL.AbrirConexion());
                cmd.Parameters.Add("@StringXML", SqlDbType.VarChar).Value = Detalle;
                cmd.CommandType = CommandType.StoredProcedure;

                conexionSQL.AbrirConexion();

                cmd.ExecuteNonQuery();
                MessageBox.Show("Registro exitoso!", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Information);
                conexionSQL.CerrarConexion();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                conexionSQL.CerrarConexion();

                throw;
            }

            
        }
    }
}
