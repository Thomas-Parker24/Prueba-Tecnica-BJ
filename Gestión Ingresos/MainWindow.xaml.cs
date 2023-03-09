using DevExpress.Utils.CommonDialogs.Internal;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Gestión_Ingresos
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SqlConnection connection = new SqlConnection("Server = (localdb)\\Main; database = BJ; integrated security = true");
        public MainWindow()
        {
            InitializeComponent();
            connection.Open();


            RellenarComboBox(TipoPersonaReportadorCBox, connection, "select tipoPersona from TipoPersona", "TipoPersona");
            RellenarComboBox(TipoPersonaCBox, connection, "Select tipoPersona from TipoPersona", "tipoPersona");
            RellenarComboBox(DependenciaCBox,connection, "select Area from Area", "Area");
            RellenarComboBox(ExcusaCBox, connection, "select valorExcusa from Excusa","valorExcusa");

            TipoEventoCBox.Items.Clear();
            TipoEventoCBox.Items.Add("Ingreso");
            TipoEventoCBox.Items.Add("Salida");

            TipoReporteCBox.Items.Clear();
            TipoReporteCBox.Items.Add("Horas trabajadas por empleado");
            TipoReporteCBox.Items.Add("Horas trabajadas por área");
            TipoReporteCBox.Items.Add("Cantidad de personas en las instalaciones");
            TipoReporteCBox.Items.Add("Histórico de Ingresos y salidas");




        }
        public void RellenarComboBox (ComboBox combo, SqlConnection conexion, string queryRC, string columna)
        {
            SqlCommand comando = new SqlCommand(queryRC, conexion);
            SqlDataReader lectorComando = comando.ExecuteReader();

            if (lectorComando.HasRows)
            {
                while (lectorComando.Read())
                {
                    combo.Items.Add(lectorComando[columna]);
                }
                lectorComando.Close();
            }
        }

        public void RellenarTabla(DataGrid tabla, SqlConnection conexion, string queryRT)
        {
            SqlDataAdapter comandoTabla = new SqlDataAdapter(queryRT, conexion);
            DataSet td = new DataSet();
            comandoTabla.Fill(td);
            tabla.ItemsSource = td.Tables[0].DefaultView;
        }

        private void GuardarUsuario(object sender, RoutedEventArgs e)
        {
            string documento = DocumentoEntry.Text;
            string nombre = NombreEntry.Text;
            string TipoPersona = TipoPersonaCBox.Text;
            int IdDependencia = 0;
            int IdTipoPersona = 0;

            string QueryTipoPersona = "select idTipoPersona from TipoPersona where tipoPersona = '" + TipoPersona + "'";
            SqlCommand getTipoPersona = new SqlCommand(QueryTipoPersona, connection);
            SqlDataReader lectorTipoPersona = getTipoPersona.ExecuteReader();

            if (lectorTipoPersona.HasRows)
            {
                while (lectorTipoPersona.Read())
                {
                    IdTipoPersona = (int)lectorTipoPersona["idTipoPersona"];
                }
            }
            lectorTipoPersona.Close();

            string consultarExistencia = "Select * from Persona where documento = " + documento;
            SqlCommand consultaExistencia = new SqlCommand(consultarExistencia, connection);
            SqlDataReader lectorExistencia = consultaExistencia.ExecuteReader();
            bool existencia = lectorExistencia.HasRows;
            lectorExistencia.Close();
            if (existencia)
            {
                string QueryUpdate = "update Persona set nombre = '" + nombre + "', idTipoPersona = " + IdTipoPersona+ " where documento = "+documento;
                SqlCommand comandoUpdate = new SqlCommand(QueryUpdate, connection);
                comandoUpdate.ExecuteNonQuery();
                MessageBox.Show("Registro actualizado correctamente", "Alerta", MessageBoxButton.OK, MessageBoxImage.Exclamation);


            }
            else
            {
                string queryInsert = "Insert Persona (documento, nombre, idTipoPersona) values ('" + documento + "','" + nombre + "'," + IdTipoPersona + ") ";
                SqlCommand IngresoDatos = new SqlCommand(queryInsert, connection);
                IngresoDatos.ExecuteNonQuery();

                MessageBox.Show("Persona almacenada con éxito", "Alerta", MessageBoxButton.OK, MessageBoxImage.Information);


                if (DependenciaCBox.SelectedItem != null && TipoPersonaCBox.SelectedValue.ToString().Equals("empleado"))
                {
                    string dependencia = (string)DependenciaCBox.SelectedItem;
                    string consultarIdDependencia = "select idArea from Area where Area ='" + dependencia + "'";
                    SqlCommand IdDependenciaQuery = new SqlCommand(consultarIdDependencia, connection);
                    SqlDataReader lectorIdDependencia = IdDependenciaQuery.ExecuteReader();


                    if (lectorIdDependencia.HasRows)
                    {
                        while (lectorIdDependencia.Read())
                        {
                            IdDependencia = (int)lectorIdDependencia["idArea"];
                        }
                        lectorIdDependencia.Close();
                    }

                    if (IdDependencia != 0)
                    {
                        string insertArea = "insert empleado (documento, Area) values (" + documento + "," + IdDependencia + ")";
                        SqlCommand insertEmpleado = new SqlCommand(insertArea, connection);
                        insertEmpleado.ExecuteNonQuery();
                    }
                }if (DependenciaCBox.SelectedItem == null && TipoPersonaCBox.SelectedValue.ToString().Equals("empleado"))
                {
                    MessageBox.Show("Ingrese la dependenica del empleado", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                

            }
            RellenarTabla(TablaPrincipal, connection, "Select * from Persona");
            DocumentoEntry.Text = "";
            NombreEntry.Text = "";
            TipoPersonaCBox.Text = "";
            DependenciaCBox.Text = "";

        }

        private void BuscarUsuario (object sender, RoutedEventArgs e)
        {
            string documento = DocumentoEntry.Text;
            int idTipoPersona = 0;
            string TipoPersonaValor = null;
            int area = 0;
            string valorArea = null;

            if (string.IsNullOrEmpty(documento))
            {
                MessageBox.Show("Debe ingresar un documento");
            }
            else
            {
                string query = "select * from Persona where documento =" + documento;
                SqlCommand comandoBusqueda = new SqlCommand(query, connection);
                SqlDataReader lectorBusqueda = comandoBusqueda.ExecuteReader();

                if (!lectorBusqueda.HasRows)
                {
                    MessageBox.Show("Documento no encontrada en la base de datos");
                    lectorBusqueda.Close();
                }
                else
                {
                    while (lectorBusqueda.Read())
                    {
                        NombreEntry.Text = (string)lectorBusqueda["nombre"];
                        idTipoPersona = (int)lectorBusqueda["idTipoPersona"];
                    }
                    lectorBusqueda.Close();
                }

                string queryValueTipoPersona = "Select tipoPersona from TipoPersona where idTipoPersona = "+idTipoPersona;
                SqlCommand comandoValorTipoPersona = new SqlCommand(queryValueTipoPersona, connection);
                SqlDataReader lectorValorTipoPersona = comandoValorTipoPersona.ExecuteReader();

                if (lectorValorTipoPersona.HasRows)
                {
                    while (lectorValorTipoPersona.Read())
                    {
                        TipoPersonaValor = (string)lectorValorTipoPersona["tipoPersona"];
                    }
                    lectorValorTipoPersona.Close();
                }

                TipoPersonaCBox.Text = TipoPersonaValor;

                if (TipoPersonaValor.Equals("empleado"))
                {
                    string queryBusquedaEmpleado = "select area from empleado where documento = " + documento;
                    SqlCommand BusquedaEmpleadoCommand = new SqlCommand(queryBusquedaEmpleado, connection);
                    SqlDataReader lectorBusquedaEmpleado = BusquedaEmpleadoCommand.ExecuteReader();

                    if (lectorBusquedaEmpleado.HasRows)
                    {
                        while (lectorBusquedaEmpleado.Read())
                        {
                            area = (int)lectorBusquedaEmpleado["area"];
                        }
                        lectorBusquedaEmpleado.Close();
                    }
                }

                if (area != 0)
                {
                    string queryBusquedaValorArea = "select Area from area where idArea = " + area;
                    SqlCommand BusquedaValorArea = new SqlCommand(queryBusquedaValorArea, connection);
                    SqlDataReader lectorValorArea = BusquedaValorArea.ExecuteReader();

                    if (lectorValorArea.HasRows)
                    {
                        while (lectorValorArea.Read())
                        {
                            valorArea = (string)lectorValorArea["Area"];
                        }
                    }
                    lectorValorArea.Close();
                }

                DependenciaCBox.Text = valorArea;

            }
        }

        private void BorrarUsuario(object sender, RoutedEventArgs e)
        {
            string documento = DocumentoEntry.Text;
            MessageBox.Show("Se eliminará el usuario con documento "+documento+"\ny todo lo relaciado con ella", "Alerta", MessageBoxButton.OK, MessageBoxImage.Question);
            string queryexiste = "select * from empleado where documento =" + documento;
            SqlCommand comandoExiste = new SqlCommand(queryexiste, connection);
            SqlDataReader lectorExiste = comandoExiste.ExecuteReader();
            bool existe = lectorExiste.HasRows;
            lectorExiste.Close();

            if (existe)
            {
                string eliminarEmpleado = "delete from Empleado where documento =" + documento;
                SqlCommand eliminarEmpleadoCommand = new SqlCommand(eliminarEmpleado, connection);
                eliminarEmpleadoCommand.ExecuteNonQuery();
            }

            string EliminarRegistros = "delete from Salida where idPersona = " + documento+"; \n delete from Ingreso where idPersona ="+documento;
            SqlCommand EliminarRegistrosCommand = new SqlCommand(EliminarRegistros, connection);
            EliminarRegistrosCommand.ExecuteNonQuery();

            string query = "delete from Persona where documento = " + documento;
            SqlCommand comandoEliminar = new SqlCommand(query, connection);
            comandoEliminar.ExecuteNonQuery();

            RellenarTabla(TablaPrincipal, connection, "Select * from Persona");
            DocumentoEvento.Text = "";
            SelectorFecha.Text = "";
            TipoEventoCBox.Text = "";

        }

        private void BorrarCampos(object sender, RoutedEventArgs e)
        {
            DocumentoEntry.Text = "";
            NombreEntry.Text = "";
            TipoPersonaCBox.Text = "";
            DependenciaCBox.Text = "";
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ( Pestañas.SelectedIndex == 0)
            {
                RellenarTabla(TablaPrincipal, connection, "select Persona.documento as 'Documento', Persona.nombre as 'Nombre', " +
                    "TipoPersona.tipoPersona as 'Tipo de persona' " +
                    "from Persona inner join TipoPersona on Persona.idTipoPersona = TipoPersona.idTipoPersona " +
                    "order by Persona.documento");
                DocumentoEvento.Text = "";
                SelectorFecha.Text = "";
                TipoEventoCBox.Text = "";
            }

        }

        private void GuardarEvento(object sender, RoutedEventArgs e)
        {
            string documento = DocumentoEvento.Text;
            string fecha = SelectorFecha.Text;
            string tipoEvento = TipoEventoCBox.Text;
            string hora = HoraEntry.Text;
            string[] horaSplit = hora.Split(':');
            int idExcusa = 0;

            string[] fechaSplit = fecha.Split('/');
            string fechaSQL = fechaSplit[1] + "/" + fechaSplit[0] + "/" + fechaSplit[2];


            string queryValidarEmpleado = "select * from empleado where documento = " + documento;
            SqlCommand comandoValidarEmpleado = new SqlCommand(queryValidarEmpleado, connection);
            SqlDataReader lectorValidarEmpleado = comandoValidarEmpleado.ExecuteReader();
            bool ValidarEmpleado = lectorValidarEmpleado.HasRows;
            lectorValidarEmpleado.Close();

            if (ValidarEmpleado && Int32.Parse(horaSplit[0]) < 16 && String.IsNullOrEmpty(ExcusaCBox.Text) && tipoEvento.Equals("Salida"))
            {
                
                MessageBox.Show("Deberá seleccionar la excusa \n y presionar nuevamente guardar", "Informacion", MessageBoxButton.OK, MessageBoxImage.Error);
                ExcusaCBox.IsEnabled = true;
            }
            if((ValidarEmpleado && Int32.Parse(horaSplit[0]) < 16 && ExcusaCBox.SelectedIndex != -1 && tipoEvento.Equals("Salida")))
            {
                string queryValorExcusa = "select idExcusa from Excusa where valorExcusa = '"+ExcusaCBox.Text+"'";
                SqlCommand commandValorExcusa = new SqlCommand(queryValorExcusa, connection);
                SqlDataReader lectorValorExcusa = commandValorExcusa.ExecuteReader();

                if (lectorValorExcusa.HasRows)
                {
                    while (lectorValorExcusa.Read())
                    {
                        idExcusa = (int)lectorValorExcusa["idExcusa"];
                    }
                    lectorValorExcusa.Close();
                }

                string insertquery = "insert salida (idPersona, fechaSalida, horaEvento, minutoEvento ,excusa) values(" + documento + ",'" + fechaSQL + "'," + horaSplit[0] +","+horaSplit[1]+","+idExcusa+")";
                SqlCommand commandInsert = new SqlCommand(insertquery, connection);
                commandInsert.ExecuteNonQuery();

                ExcusaCBox.Text = "";
                ExcusaCBox.IsEnabled = false;

            }
            if (Int32.Parse(horaSplit[0]) >= 16  || !ValidarEmpleado)
            {
                string query = "Insert "+ tipoEvento +" (idPersona, fecha"+ tipoEvento +", horaEvento, minutoEvento) values (" + documento + ",'" + fechaSQL + "'," + horaSplit[0] + "," + horaSplit[1]+")";
                SqlCommand comando = new SqlCommand(query, connection);
                comando.ExecuteNonQuery();

                MessageBox.Show("Registro guardado correctamente", "Alerta", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            if (tipoEvento.Equals("Ingreso")) { 
                
                string query = "Insert ingreso (idPersona, fechaIngreso, horaEvento, minutoEvento) values" +
                    " (" + documento + ",'" + fechaSQL + "'," + horaSplit[0] + "," + horaSplit[1]+")";
                SqlCommand comandoIngreso = new SqlCommand(query, connection);
                comandoIngreso.ExecuteNonQuery();
                MessageBox.Show("Registro guardado correctamente", "Alerta", MessageBoxButton.OK, MessageBoxImage.Information);

            }

        }
        private void LimpiarEventos(object sender, RoutedEventArgs e)
        {
            DocumentoEvento.Text = "";
            SelectorFecha.Text = "";
            TipoEventoCBox.Text = "";
        }


        private void SelectorFecha_MouseLeave(object sender, MouseEventArgs e)
        {
            string documento = DocumentoEvento.Text;

            if (!string.IsNullOrEmpty(documento))
            {
                string query =
                    "select Ingreso.idPersona as 'Documento', Persona.nombre as 'Nombre', TipoPersona.tipoPersona as " +
                    "'Tipo de Persona',  Ingreso.fechaIngreso as 'Fecha de ingreso', Ingreso.horaEvento as 'Hora de Ingreso', " +
                    "Ingreso.MinutoEvento as 'Minuto de Ingreso', Salida.fechaSalida as 'Fecha de Salida', Salida.horaEvento as 'Hora de Salida'," +
                    "Salida.MinutoEvento as 'Minuto de Salida', Salida.excusa as 'Excusa' " +
                    "from Ingreso inner join Salida on Ingreso.idPersona ="+documento+ " and Salida.idPersona = "+documento+
                    "and Ingreso.fechaIngreso = Salida.fechaSalida inner join Persona on Persona.documento = " +
                    "Ingreso.idPersona inner join TipoPersona on Persona.idTipoPersona = TipoPersona.idTipoPersona";

                RellenarTabla(TablaEvetos, connection, query);
            }

            
        }

        private void GeneradorReportesButton(object sender, RoutedEventArgs e)
        {
            string where = "where ";

            switch (TipoReporteCBox.SelectedValue)
            {
                case "Horas trabajadas por empleado":
                    string queryHorasTrabajador = "select Ingreso.idPersona as 'Documento', Persona.nombre as 'Nombre' " +
                                                ",sum ((Salida.horaEvento* 60 - Ingreso.horaEvento* 60) + (Salida.MinutoEvento - Ingreso.MinutoEvento)/60) as " +
                                                "'Tiempo trabajado en minutos', Area.Area as 'Area'  from Ingreso inner join Salida on Ingreso.idPersona = Salida.idPersona inner " +
                                                "join Persona on Ingreso.idPersona = Persona.documento inner join Empleado on Empleado.documento = Persona.documento inner join " +
                                                "Area on Area.idArea = Empleado.area  inner join TipoPersona on Persona.idTipoPersona = TipoPersona.idTipoPersona " +
                                                "where TipoPersona.tipoPersona ='empleado' " +
                                                "group by Ingreso.idPersona, Persona.nombre, Area.Area";

                    RellenarTabla(TablaReportador, connection, queryHorasTrabajador);
                    break;

                case "Horas trabajadas por área":
                    string queryHorasArea = 
                        queryHorasArea = "select Area.Area as 'Area', Sum ((Salida.horaEvento* 60 - Ingreso.horaEvento* 60) + " +
                        "(Salida.MinutoEvento - Ingreso.MinutoEvento)/60) as 'Minutos Trabajados' from Ingreso inner join Salida on Ingreso.idPersona " +
                        "= Salida.idPersona inner join Empleado on Ingreso.idPersona = Empleado.documento inner join Area on Empleado.area = " +
                        "Area.idArea group by Area.Area";
                    RellenarTabla(TablaReportador, connection, queryHorasArea);
                    break;

                case "Cantidad de personas en las instalaciones":

                    string queryConsultaEmergencia = "select Ingreso.idPersona as 'documento', Persona.nombre as 'Nombre', " +
                        "TipoPersona.tipoPersona as 'Tipo de Persona' , Ingreso.fechaIngreso as 'Fecha de Ingreso', " +
                        "Ingreso.horaEvento as 'Hora de Ingreso' from Ingreso full outer join Salida on Ingreso.idPersona = Salida.idPersona " +
                        "And Ingreso.fechaIngreso = Salida.fechaSalida " +
                        "inner join Persona on Ingreso.idPersona = Persona.documento " +
                        "inner join TipoPersona on Persona.idTipoPersona = TipoPersona.idTipoPersona where Salida.fechaSalida is null";

                    RellenarTabla(TablaReportador, connection, queryConsultaEmergencia);
                    break;

                case "Histórico de Ingresos y salidas":
                    string queryConsultaHistoricos = null;

                    
                             if (TipoPersonaReportadorCBox.SelectedItem != null)
                    {
                        string seleccion = (string)TipoPersonaReportadorCBox.SelectedValue;
                        where += "TipoPersona.tipoPersona = '" + seleccion + "'";
                    }
                    if (DesdeDatePicker.SelectedDate != null)
                    {
                        string seleccionDesdeDate = DesdeDatePicker.Text;
                        string[] fechaSplit = seleccionDesdeDate.Split("/");
                        string fechaSQL = fechaSplit[1] + "/" + fechaSplit[0] + "/" + fechaSplit[2];

                        where += " And Ingreso.fechaIngreso >= '" + fechaSQL + "'";

                    }
                    if (HastaDatePicker.SelectedDate != null)
                    {
                        string seleccionDesdeDate = DesdeDatePicker.Text;
                        string[] fechaSplit = seleccionDesdeDate.Split("/");
                        string fechaSQL = fechaSplit[1] + "/" + fechaSplit[0] + "/" + fechaSplit[2];

                        where += " And Ingreso.fechaIngreso >= '" + fechaSQL + "'";
                    }


                    if (where.Equals("where "))
                    {
                        queryConsultaHistoricos = "select Ingreso.idPersona as 'Documento', Persona.nombre as 'Nombre', TipoPersona.tipoPersona as " +
                        "'Tipo de Persona',  Ingreso.fechaIngreso as 'Fecha de ingreso', Ingreso.horaEvento as 'Hora de Ingreso', " +
                        "Ingreso.MinutoEvento as 'Minuto de Ingreso', Salida.fechaSalida as 'Fecha de Salida', Salida.horaEvento as 'Hora de Salida'," +
                        "Salida.MinutoEvento as 'Minuto de Salida', Salida.excusa as 'Excusa' " +
                        "from Ingreso inner join Salida on Ingreso.idPersona = Salida.idPersona " +
                        "and Ingreso.fechaIngreso = Salida.fechaSalida inner join Persona on Persona.documento = " +
                        "Ingreso.idPersona inner join TipoPersona on Persona.idTipoPersona = TipoPersona.idTipoPersona";

                    }
                    else
                    {
                        queryConsultaHistoricos = "select Ingreso.idPersona as 'Documento', Persona.nombre as 'Nombre', TipoPersona.tipoPersona as " +
                        "'Tipo de Persona',  Ingreso.fechaIngreso as 'Fecha de ingreso', Ingreso.horaEvento as 'Hora de Ingreso', " +
                        "Ingreso.MinutoEvento as 'Minuto de Ingreso', Salida.fechaSalida as 'Fecha de Salida', Salida.horaEvento as 'Hora de Salida'," +
                        "Salida.MinutoEvento as 'Minuto de Salida', Salida.excusa as 'Excusa' " +
                        "from Ingreso inner join Salida on Ingreso.idPersona = Salida.idPersona " +
                        "and Ingreso.fechaIngreso = Salida.fechaSalida inner join Persona on Persona.documento = " +
                        "Ingreso.idPersona inner join TipoPersona on Persona.idTipoPersona = TipoPersona.idTipoPersona "+ where;
                    }

                    RellenarTabla(TablaReportador, connection, queryConsultaHistoricos);

                    break;

            }
        }


        private void DependenciaCBox_MouseLeave(object sender, MouseEventArgs e)
        {
            DependenciaCBox.IsEnabled = TipoPersonaCBox.SelectedIndex != -1 && TipoPersonaCBox.SelectedValue.Equals("empleado");
        }

        private void TipoPersonaCBox_MouseLeave(object sender, MouseEventArgs e)
        {
            DependenciaCBox.IsEnabled = TipoPersonaCBox.SelectedIndex != -1 && TipoPersonaCBox.SelectedValue.Equals("empleado");
        }

        private void touchTest(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("La hora debe ser escrita en formato 24 horas \n y con el siguiente formato hh:mm", "Alerta", MessageBoxButton.OK, MessageBoxImage.Warning);

        }

        private void alerta(object sender, EventArgs e)
        {
            MessageBox.Show("Para activar los filtros debe seleccionar \nel reporte del histórico", "Alerta", MessageBoxButton.OK, MessageBoxImage.Exclamation);

        }

        private void ActualizarComponentes(object sender, MouseEventArgs e)
        {
                if(TipoReporteCBox.Text.Equals("Histórico de Ingresos y salidas"))
                {
                    DesdeDatePicker.IsEnabled = true;
                    HastaDatePicker.IsEnabled = true;
                    TipoPersonaReportadorCBox.IsEnabled = true;
                }
            else
            {
                DesdeDatePicker.IsEnabled = false;
                HastaDatePicker.IsEnabled = false;
                TipoPersonaReportadorCBox.IsEnabled = false;
            }
        }
    }
}
