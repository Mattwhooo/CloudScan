using System;
using System.Collections.Generic;
using System.Linq;
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

namespace CloudScan
{
    /// <summary>
    /// Interaction logic for AddEmail.xaml
    /// </summary>
    public partial class AddEmail : Window
    {
        public string emailString
        {
            get;
            set;
        }
        public AddEmail()
        {
            InitializeComponent();
        }

        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            emailString = emailTextBox.Text;
            this.Close();
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            emailString = null;
            this.Close();
        }

    }
}
