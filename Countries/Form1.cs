using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;
using Dapper.Contrib.Extensions;
using Dapper;

namespace Countries
{
    public partial class Form1 : Form
    {
        HttpClient client;
        string sqlconnection;
        List<Country> info = null;
        ShowDB showdb;

        public Form1()
        {
            InitializeComponent();
            client = new HttpClient();
            client.BaseAddress = new Uri("https://restcountries.eu/rest/v2/name/");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            sqlconnection = "Server=.\\SQLEXPRESS;Initial Catalog=Countries;Integrated Security=true";
        }

        private async void srchButton_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            await Task.Run(() => info = RunAsync(textBox1.Text, client).GetAwaiter().GetResult());
            if (info != null)
            {
                richTextBox1.AppendText(info[0].ToString());
                var save = MessageBox.Show("Желаете сохранить/обновить страну в БД?", "Сохранение страны в БД", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (save == DialogResult.Yes)
                {
                    using (SqlConnection connection = new SqlConnection(sqlconnection))
                    {
                        var addCountry = new CountriesDB
                        {
                            Name = info[0].Name,
                            Alpha2Code = info[0].Alpha2Code,
                            Population = info[0].Population,
                            Area = info[0].Area
                        };

                        var addRegion = new Regions
                        {
                            Name = info[0].Region
                        };

                        // проверка на начиличие региона
                        var existreg = connection.ExecuteScalar<bool>("select count(1) from Regions where Name = @Name", new { addRegion.Name });
                        if (!existreg)
                        {
                            connection.Insert(addRegion);
                        }

                        var getreg = connection.QueryFirst<Regions>("select * from Regions where Name = @Name", new { addRegion.Name });
                        addCountry.Region = getreg.ID;

                        var addCapital = new Cities
                        {
                            Name = info[0].Capital
                        };

                        // проверка на наличие столицы
                        var existcap = connection.ExecuteScalar<bool>("select count(1) from Cities where Name = @Name", new { addCapital.Name });

                        if (!existcap)
                        {
                            connection.Insert(addCapital);
                        }

                        var getcap = connection.QueryFirst<Cities>("select * from Cities where Name = @Name", new { addCapital.Name });
                        addCountry.Capital = getcap.ID;

                        // провека на наличие страны
                        var existcntry = connection.ExecuteScalar<bool>("select count(1) from Countries where Alpha2Code = @Alpha2Code", new { addCountry.Alpha2Code });
                        if (existcntry)
                        {
                            connection.Update(addCountry);
                            MessageBox.Show("Данные обновлены", "Успех");
                        }
                        else
                        {
                            connection.Insert(addCountry);
                            MessageBox.Show("Данные добавлены", "Успех");
                        }
                    }
                }
            }
        }
        static async Task<List<Country>> RunAsync(string name, HttpClient client)
        {
            try
            {
                string responseBody = await client.GetStringAsync(client.BaseAddress.AbsoluteUri + name);
                var country = JsonConvert.DeserializeObject<List<Country>>(responseBody);
                return country;
            }
            catch
            {
                MessageBox.Show("Страна не найдена");
                return null;
            }

        }
        public class Country
        {
            public string Name { get; set; }
            public string Alpha2Code { get; set; }
            public string Capital { get; set; }
            public double Area { get; set; }
            public int Population { get; set; }
            public string Region { get; set; }
            public override string ToString()
            {
                return "Название: " + Name + "\nКод страны: " + Alpha2Code + "\nСтолица: " + Capital + "\nПлощадь: " + Area
                    + "\nНаселение: " + Population + "\nРегион: " + Region;
            }

        }

        private void showButton_Click(object sender, EventArgs e)
        {
            showdb = new ShowDB();
            using (SqlConnection connection = new SqlConnection(sqlconnection))
            {
                int i = 0;
                var getdb = connection.GetAll<CountriesDB>();
                foreach (var country in getdb)
                {
                    showdb.DB.Rows.Add();
                    showdb.DB.Rows[i].Cells[0].Value = country.Name;
                    showdb.DB.Rows[i].Cells[1].Value = country.Alpha2Code;
                    int ID1 = country.Capital;
                    var getcap = connection.QueryFirst<Cities>("select * from Cities where ID = @ID1", new { ID1 });
                    showdb.DB.Rows[i].Cells[2].Value = getcap.Name;
                    showdb.DB.Rows[i].Cells[3].Value = country.Area;
                    showdb.DB.Rows[i].Cells[4].Value = country.Population;
                    int ID2 = country.Region;
                    var getreg = connection.QueryFirst<Regions>("select * from Regions where ID = @ID2", new { ID2 });
                    showdb.DB.Rows[i].Cells[5].Value = getreg.Name;
                    i++;
                }
            }
            showdb.Show();
        }
    }
}
