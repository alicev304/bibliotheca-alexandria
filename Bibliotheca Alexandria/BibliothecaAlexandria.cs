using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Bibliotheca_Alexandria
{
    public partial class BibliothecaAlexandria : Form
    {
        public static void main(string[] args)
        {
            Application.Run(new BibliothecaAlexandria());
        }

        public BibliothecaAlexandria()
        {
            InitializeComponent();
        }

        private void Registration_Click(object sender, EventArgs e)
        {

        }

        private void exit_label_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void register_label_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ssnbox.Text) || string.IsNullOrWhiteSpace(fnamebox.Text) || string.IsNullOrWhiteSpace(lnamebox.Text)
                || string.IsNullOrWhiteSpace(addrbox.Text) || string.IsNullOrWhiteSpace(emailbox.Text) || string.IsNullOrWhiteSpace(citybox.Text)
                || string.IsNullOrWhiteSpace(statebox.Text) || string.IsNullOrWhiteSpace(phonebox.Text))
            {
                MessageBox.Show("Registration failed. \nAll fields are mandatory.");
                return;
            }
                string reg_query = "INSERT INTO borrower (ssn, first_name, last_name, address, email, city, state, phone) VALUES (\"" 
                + ssnbox.Text + "\", \"" + fnamebox.Text + "\", \"" + lnamebox.Text + "\", \"" + addrbox.Text + "\", \"" + emailbox.Text 
                + "\", \"" + citybox.Text + "\", \"" + statebox.Text + "\", \"" + phonebox.Text + "\");";
            string id_query = "SELECT card_id FROM borrower WHERE ssn = \"" + ssnbox.Text + "\";";
            string SSNcount = "";
            try
            {
                MySqlConnection con = new MySqlConnection("server=localhost;user id=root;password=inspiron15r;database=lms");
                con.Open();
                MySqlCommand mscom = new MySqlCommand("SELECT count(*) FROM borrower WHERE ssn = \"" + ssnbox.Text + "\";", con);
                using (MySqlDataReader reader = mscom.ExecuteReader())
                {
                    reader.Read();
                    SSNcount = reader.GetString(0);
                }
                if (SSNcount == "1")
                {
                    MessageBox.Show("Registration failed. SSN entered is already registered.");
                }
                else
                {
                    mscom = new MySqlCommand(reg_query, con);
                    mscom.ExecuteNonQuery();
                    mscom = new MySqlCommand(id_query, con);
                    using (MySqlDataReader reader = mscom.ExecuteReader())
                    {
                        if (reader.Read()) MessageBox.Show("New borrower registered.\nPlease note your Borrower Card ID: " + reader.GetString(0));
                        else MessageBox.Show("Registration failed.");
                    }
                }
                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Registration failed.\n" + ex.Message);
            }
        }

        private void exit_button_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void search_button_Click(object sender, EventArgs e)
        {
            string search_string = searchbox.Text;
            string[] keywords = search_string.Split();

            string[] temp_kw = new string[3];
            if (keywords.Length == 1)
            {
                temp_kw[0] = keywords[0];
                temp_kw[1] = "";
                temp_kw[2] = "";
                keywords = temp_kw;
            }
            else if (keywords.Length == 2)
            {
                temp_kw[0] = keywords[0];
                temp_kw[1] = keywords[1];
                temp_kw[2] = "";
                keywords = temp_kw;
            }

            string query_builder1 = "(";
            string query_builder2 = "(";
            string query_builder3 = "(";
            for (int i = 0; i < keywords.Length; i++)
            {
                for (int j = 0; j < keywords.Length; j++)
                {
                    if (j == i) continue;
                    for (int k = 0; k < keywords.Length; k++)
                    {
                        if (k == j || k == i) continue;
                        if (query_builder1.Length > 1) query_builder1 += " OR ";
                        string a, b, c;
                        a = "book.ISBN10 LIKE \"%" + keywords[i] + "%\"";
                        b = "authors.name LIKE \"%" + keywords[j] + "%\"";
                        c = "book.title LIKE \"%" + keywords[k] + "%\"";
                        query_builder1 += "(" + a + " AND " + b + " AND " + c + ")";
                    }
                }
            }
            query_builder1 += ")";
            for (int i = 0; i < keywords.Length; i++)
            {
                for (int j = 0; j < keywords.Length; j++)
                {
                    if (j == i) continue;
                    for (int k = 0; k < keywords.Length; k++)
                    {
                        if (k == j || k == i) continue;
                        if (query_builder2.Length > 1) query_builder2 += " OR ";
                        string a, b, c;
                        a = "book.ISBN13 LIKE \"%" + keywords[i] + "%\"";
                        b = "authors.name LIKE \"%" + keywords[j] + "%\"";
                        c = "book.title LIKE \"%" + keywords[k] + "%\"";
                        query_builder2 += "(" + a + " AND " + b + " AND " + c + ")";
                    }
                }
            }
            query_builder2 += ")";
            for (int i = 0; i < keywords.Length; i++)
            {
                if (i > 0) query_builder3 += " AND ";
                query_builder3 += "book.title LIKE \"%" + keywords[i] + "%\"";
            }
            query_builder3 += ") OR (";
            for (int i = 0; i < keywords.Length; i++)
            {
                if (i > 0) query_builder3 += " AND ";
                query_builder3 += "authors.name LIKE \"%" + keywords[i] + "%\"";
            }
            query_builder3 += ")";

            string search_query = "SELECT distinct book.ISBN10, book.ISBN13, book.title AS \"Title\", authors.name AS \"Author\", " 
                + "book.availability AS \"Availability\" FROM book NATURAL JOIN book_authors NATURAL JOIN authors WHERE "
                + query_builder1 + " OR " + query_builder2 + " OR " + query_builder3;
            Console.WriteLine(search_query);
            try
            {
                MySqlConnection con = new MySqlConnection("server=localhost;user id=root;password=inspiron15r;database=lms");
                con.Open();
                MySqlCommand mscom = new MySqlCommand(search_query, con);
                MySqlDataAdapter sda = new MySqlDataAdapter();
                sda.SelectCommand = mscom;
                DataTable res = new DataTable();
                sda.Fill(res);
                BindingSource bSource = new BindingSource();
                bSource.DataSource = res;
                search_results.DataSource = bSource;
                sda.Update(res);
                if (search_results.RowCount == 0)
                {
                    checkout_button.Enabled = false;
                }
                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void searchbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                search_button.PerformClick();
            }
        }

        private void borrower_details_button_Click(object sender, EventArgs e)
        {
            string query_verify_cardID = "SELECT count(*) FROM borrower WHERE card_id = \"" + card_ID_box.Text + "\";";
            string query_active = "SELECT loan_id AS \"Loan ID\", ISBN10, ISBN13, date_out AS \"Date Out\", due_date AS \"Due Date\" FROM book_loans "
                + "WHERE date_in IS NULL and card_id = " + card_ID_box.Text;
            string query_history = "SELECT loan_id AS \"Loan ID\", ISBN10, ISBN13, date_out AS \"Date Out\", due_date AS \"Due Date\", date_in AS \"Date In\" "
                + "FROM book_loans WHERE date_in IS NOT NULL and card_id = " + card_ID_box.Text;
            string cardIDCount = "";
            try
            {
                MySqlConnection con = new MySqlConnection("server=localhost;user id=root;password=inspiron15r;database=lms;");
                con.Open();
                MySqlCommand mscom = new MySqlCommand(query_verify_cardID, con);
                using (MySqlDataReader reader = mscom.ExecuteReader())
                {
                    reader.Read();
                    cardIDCount = reader.GetString(0);
                }
                if (cardIDCount == "0")
                {
                    MessageBox.Show("Card ID invalid.");
                }
                else
                {
                    mscom = new MySqlCommand("SELECT first_name, last_name FROM borrower WHERE card_id = " + card_ID_box.Text, con);
                    using (MySqlDataReader reader = mscom.ExecuteReader())
                    {
                        reader.Read();
                        borrower_name.Text = "Name: " + reader.GetString(0) + " " + reader.GetString(1);
                        borrower_name.Visible = true;
                    }
                    mscom = new MySqlCommand(query_active, con);
                    MySqlDataAdapter sda = new MySqlDataAdapter();
                    sda.SelectCommand = mscom;
                    DataTable res = new DataTable();
                    sda.Fill(res);
                    BindingSource bSource = new BindingSource();
                    bSource.DataSource = res;
                    active_results.DataSource = bSource;
                    sda.Update(res);
                    mscom = new MySqlCommand(query_history, con);
                    sda = new MySqlDataAdapter();
                    sda.SelectCommand = mscom;
                    res = new DataTable();
                    sda.Fill(res);
                    bSource = new BindingSource();
                    bSource.DataSource = res;
                    history_results.DataSource = bSource;
                    sda.Update(res);
                }
                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void exit_button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void close_button_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button2_Click(sender, e);
            string query_verify_cardID = "SELECT count(*) FROM borrower WHERE card_id = \"" + card_ID_box.Text + "\";";
            string cardIDCount = "";
            string query_fine_history = "SELECT book_loans.loan_id AS \"Loan ID\", book_loans.ISBN10, book_loans.ISBN13, book_loans.date_out AS \"Date Out\", "
                + "book_loans.due_date AS \"Due Date\", book_loans.date_in AS \"Date In\", FORMAT(fines.fine_amount, 2) AS \"Fine Amount ($)\" " 
                + "FROM book_loans NATURAL JOIN fines WHERE book_loans.card_id = \"" + textBox1.Text + "\" AND fines.paid = 'YES';";
            string query_fine = "SELECT book_loans.loan_id AS \"Loan ID\", book_loans.ISBN10, book_loans.ISBN13, book_loans.date_out AS \"Date Out\", "
                + "FORMAT(fines.fine_amount, 2) AS \"Fine Amount ($)\" FROM book_loans NATURAL JOIN fines "
                + "WHERE book_loans.card_id = \"" + textBox1.Text + "\" AND fines.paid = 'NO';";
            string query_total_fine = "SELECT FORMAT(COALESCE(SUM(fine_amount)), 2) FROM fines NATURAL JOIN book_loans WHERE paid = \'NO\' AND card_id = \""
                + textBox1.Text + "\";";
            string query_applicable_fine = "SELECT FORMAT(COALESCE(SUM(fine_amount)), 2) FROM fines NATURAL JOIN book_loans WHERE paid = \'NO\' AND " 
                + " date_in IS NOT NULL AND card_id = \"" + textBox1.Text + "\";";
            string query_fine_paid_till_now = "SELECT FORMAT(COALESCE(SUM(fine_amount)), 2) FROM fines NATURAL JOIN book_loans WHERE paid = \'YES\' AND card_id = \""
                + textBox1.Text + "\";";
            try
            {
                MySqlConnection con = new MySqlConnection("server=localhost;user id=root;password=inspiron15r;database=lms;");
                con.Open();
                MySqlCommand mscom = new MySqlCommand(query_verify_cardID, con);
                using (MySqlDataReader reader = mscom.ExecuteReader())
                {
                    reader.Read();
                    cardIDCount = reader.GetString(0);
                }
                if (cardIDCount == "0")
                {
                    MessageBox.Show("Card ID invalid.");
                }
                else
                {
                    mscom = new MySqlCommand("SELECT first_name, last_name FROM borrower WHERE card_id = \"" + textBox1.Text + "\";", con);
                    using (MySqlDataReader reader = mscom.ExecuteReader())
                    {
                        reader.Read();
                        label5.Text = "Name: " + reader.GetString(0) + " " + reader.GetString(1);
                        label5.Visible = true;
                    }
                    mscom = new MySqlCommand(query_fine, con);
                    MySqlDataAdapter sda = new MySqlDataAdapter();
                    sda.SelectCommand = mscom;
                    DataTable res = new DataTable();
                    sda.Fill(res);
                    BindingSource bSource = new BindingSource();
                    bSource.DataSource = res;
                    dataGridView2.DataSource = bSource;
                    sda.Update(res);
                    mscom = new MySqlCommand(query_fine_history, con);
                    sda = new MySqlDataAdapter();
                    sda.SelectCommand = mscom;
                    res = new DataTable();
                    sda.Fill(res);
                    bSource = new BindingSource();
                    bSource.DataSource = res;
                    dataGridView1.DataSource = bSource;
                    sda.Update(res);
                    mscom = new MySqlCommand(query_total_fine, con);
                    using (MySqlDataReader reader = mscom.ExecuteReader())
                    {
                        reader.Read();
                        if (reader.IsDBNull(0))
                        {
                            label7.Text = "Total Fine Amount: 0.00$";
                        }
                        else
                        {

                            label7.Text = "Total Fine Amount: " + reader.GetString(0) + " $";
                        }
                        label7.Visible = true;
                    }
                    mscom = new MySqlCommand(query_applicable_fine, con);
                    using (MySqlDataReader reader = mscom.ExecuteReader())
                    {
                        reader.Read();
                        if (reader.IsDBNull(0))
                        {
                            label11.Text = "Fine Amount Payable: 0.00$";
                        }
                        else
                        {

                            label11.Text = "Fine Amount Payable: " + reader.GetString(0) + " $";
                        }
                        label11.Visible = true;
                    }
                    mscom = new MySqlCommand(query_fine_paid_till_now, con);
                    using (MySqlDataReader reader = mscom.ExecuteReader())
                    {
                        reader.Read();
                        if (reader.IsDBNull(0))
                        {
                            label13.Text = "Total Fine Paid: 0.00$";
                        }
                        else
                        {

                            label13.Text = "Total Fine Paid: " + reader.GetString(0) + " $";
                        }
                        label13.Visible = true;
                    }
                }
                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void search_results_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                checkout_button.Enabled = true;
            }
        }

        private void checkout_button_Click(object sender, EventArgs e)
        {
            if (search_results.SelectedRows.Count > 0)
            {
                if (search_results.SelectedRows[0].Cells["Availability"].Value.ToString() == "NO")
                {
                    MessageBox.Show("The selected book is not available for check out.");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(cardIDbox.Text))
                    {
                        MessageBox.Show("Please enter your Card ID.");
                    }
                    else
                    {
                        string query_count_active_loans = "SELECT COUNT(*) FROM book_loans WHERE card_id = \"" + cardIDbox.Text 
                            + "\" and date_in IS NULL;";
                        string ISBN10 = search_results.SelectedRows[0].Cells["ISBN10"].Value.ToString();
                        string ISBN13 = search_results.SelectedRows[0].Cells["ISBN13"].Value.ToString();
                        Console.WriteLine(ISBN10 + " " + ISBN13);
                        Console.ReadLine();
                        string query_checkout_update_availability = "UPDATE book SET Availability = 'NO' WHERE ISBN10 = \"" 
                            + ISBN10 + "\" AND ISBN13 = \"" + ISBN13 + "\";";
                        string query_checkout_add_loan = "INSERT INTO book_loans (ISBN10, ISBN13, card_id, date_out, due_date) VALUES (\"" 
                            + ISBN10 + "\", \"" + ISBN13 + "\", \"" + cardIDbox.Text + "\", CURRENT_DATE(), DATE_ADD(CURRENT_DATE(), INTERVAL 14 DAY));";
                        try
                        {
                            MySqlConnection con = new MySqlConnection("server=localhost;user id=root;password=inspiron15r;database=lms;");
                            con.Open();
                            MySqlCommand mscom = new MySqlCommand("SELECT first_name, last_name FROM borrower WHERE card_id = " + cardIDbox.Text, con);
                            using (MySqlDataReader reader = mscom.ExecuteReader())
                            {
                                reader.Read();
                                label9.Text = "Name: " + reader.GetString(0) + " " + reader.GetString(1);
                                label9.Visible = true;
                            }
                            mscom = new MySqlCommand(query_count_active_loans, con);
                            string active_loans = "";
                            using (MySqlDataReader reader = mscom.ExecuteReader())
                            {
                                reader.Read();
                                active_loans = reader.GetString(0);
                            }
                            if (active_loans == "3")
                            {
                                MessageBox.Show("Failed to Checkout. \nBorrower already has three active loans.");
                            }
                            else
                            {
                                mscom = new MySqlCommand(query_checkout_update_availability, con);
                                mscom.ExecuteNonQuery();
                                mscom = new MySqlCommand(query_checkout_add_loan, con);
                                mscom.ExecuteNonQuery();
                                MessageBox.Show("Book checked out successfully.");
                            }
                            con.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                }
            }
        }

        private void checkin_button_Click(object sender, EventArgs e)
        {
            if (active_results.SelectedRows.Count > 0)
            {
                string ISBN10 = active_results.SelectedRows[0].Cells["ISBN10"].Value.ToString();
                string ISBN13 = active_results.SelectedRows[0].Cells["ISBN13"].Value.ToString();
                string loan_id = active_results.SelectedRows[0].Cells["Loan ID"].Value.ToString();
                string query_update_loan = "UPDATE book_loans SET date_in = CURRENT_DATE() WHERE loan_id = \"" + loan_id + "\";";
                string query_update_availability = "UPDATE book SET Availability = 'YES' WHERE ISBN10 = \""
                            + ISBN10 + "\" AND ISBN13 = \"" + ISBN13 + "\";";
                try
                {
                    MySqlConnection con = new MySqlConnection("server=localhost;user id=root;password=inspiron15r;database=lms;");
                    con.Open();
                    MySqlCommand mscom = new MySqlCommand(query_update_loan, con);
                    mscom.ExecuteNonQuery();
                    mscom = new MySqlCommand(query_update_availability, con);
                    mscom.ExecuteNonQuery();
                    MessageBox.Show("Book checked in successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                borrower_details_button_Click(sender, e);
            }
        }

        private void active_results_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                checkin_button.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string query_update_fine = "INSERT INTO fines (loan_id, fine_amount) SELECT loan_id, ROUND(DATEDIFF(CURRENT_DATE(), due_date) * 0.25, 2) " 
                + "FROM book_loans WHERE date_in IS NULL AND CURRENT_DATE() > due_date ON DUPLICATE KEY UPDATE fines.loan_id = fines.loan_id;";
            try
            {
                MySqlConnection con = new MySqlConnection("server=localhost;user id=root;password=inspiron15r;database=lms;");
                con.Open();
                MySqlCommand mscom = new MySqlCommand(query_update_fine, con);
                mscom.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void pay_fine_Click(object sender, EventArgs e)
        {
            if (dataGridView2.SelectedRows.Count > 0)
            {
                string loanID = dataGridView2.SelectedRows[0].Cells["Loan ID"].Value.ToString();
                string query_check_book_returned = "SELECT date_in FROM book_loans WHERE loan_id = \"" + loanID + "\";";
                string query_pay_fine = "UPDATE fines SET paid = 'YES' WHERE loan_id = \"" + loanID + "\";";
                try
                {
                    MySqlConnection con = new MySqlConnection("server=localhost;user id=root;password=inspiron15r;database=lms;");
                    con.Open();
                    MySqlCommand mscom = new MySqlCommand(query_check_book_returned, con);
                    bool book_return_check = true;
                    using (MySqlDataReader reader = mscom.ExecuteReader())
                    {
                        reader.Read();
                        if (reader.IsDBNull(0))
                        {
                            book_return_check = false;
                        }
                    }
                    if (book_return_check)
                    {
                        mscom = new MySqlCommand(query_pay_fine, con);
                        mscom.ExecuteNonQuery();
                        MessageBox.Show("Fine paid successfully.");
                    }
                    else
                    {
                        MessageBox.Show("Failed to clear the fine. \nThe selected book has not been returned by the borrower.");
                    }
                    con.Close();
                    button1_Click(sender, e);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                pay_fine.Enabled = true;
            }
        }
    }
}
