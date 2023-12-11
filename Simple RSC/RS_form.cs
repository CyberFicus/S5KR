using System.Diagnostics;

namespace Simple_RSC
{
    public partial class RS_form : Form
    {
        RsContext context;
        bool? dbflag = null;

        RSC RSC;

        public RS_form()
        {
            InitializeComponent();

            RSC = new RSC(this);
            context = new RsContext();
            dblabel.Text = "";
        }

        private void start_btn_Click(object sender, EventArgs e)
        {
            try
            {
                RSC.RunString(input_textBox.Text);
            }
            catch (System.Exception ex)
            {
                logexc(ex);
            }
        }

        private void txtstart_btn_Click(object sender, EventArgs e)
        {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.Title = "������� ���� �� ������� �����";
            OFD.Filter = "txt|*.txt";
            if (OFD.ShowDialog() == DialogResult.OK)
            {
                var str = File.ReadLines(OFD.FileName).First();
                input_textBox.Text = str;
                RSC.RunString(str);
            }
            OFD.Dispose();
        }

        public void logexc(System.Exception exc)
        {
            if (!dbflag.HasValue) {
                try
                {
                    var res = context.Exceptions.ToList();
                    dblabel.Text = "����������� � �� �������";
                    dbflag = true;
                }
                catch
                {
                    dbflag = false;
                    dblabel.Text = "�� ������� ������������ � ��";
                }
            }

            if (dbflag == true)
            {
                try
                {
                    context.Exceptions.Add(new Exception { InputString = input_textBox.Text, Message = exc.Message, StackTrace = exc.StackTrace });
                    context.SaveChanges();
                    MessageBox.Show("��������� ���������� ���� ������ ������� � ��");
                }
                catch (System.Exception ex)
                {
                    while (ex.InnerException != null) { ex = ex.InnerException; }
                    MessageBox.Show("�������� ����������: " + exc.Message + "\n" + "��� ������� ������ ���� ���������� � �� ����� �������� ����������: " + ex.Message);
                }
            }
        }

        public void clear()
        {
            RichTextBox.Text = "";
        }

        public void write(string str)
        {
            RichTextBox.Text += str;
        }

        public void focus()
        {
           input_textBox.Focus();
        }

        private void btn_save_txt_Click(object sender, EventArgs e)
        {
            SaveFileDialog SFD = new SaveFileDialog();
            SFD.Title = "��������� ���� � ������������";
            SFD.Filter = "txt|*.txt";

            if (SFD.ShowDialog(this) == DialogResult.OK)
            {
                File.WriteAllText(SFD.FileName, RichTextBox.Text);

                var name = SFD.FileName;

                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = "notepad.exe";
                    psi.Arguments = $"\"{name}\"";
                    psi.UseShellExecute = true;
                    psi.CreateNoWindow = true;
                    Process.Start(psi);
                }
                catch
                {
                    MessageBox.Show("�� ������� ������� ���������� txt ����");
                }
            }

            SFD.Dispose();
        }
    }
}