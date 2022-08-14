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

namespace RVT_UTILITY
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        // TODO: output optimizations should be keyvaluepairs (line & length), opposed to just line
        // TODO: cleanup non-ideal optimizations (having duplicate 2 liners)
        // TODO: arrange the output lines to check for overlaps 
        // TODO: optimize the line counter, it seems to be very laggy and in some instances seems to set too many lines
        // TODO: auto write optimizations

        // TODO: autocomplete
        // TODO: trigger/condition/action counter
        // TODO: expand/collapse code sections
        // TODO: syntax highlighting 
        // TODO: autosaving
        // TODO: RVT hook

        
        public Dictionary<KeyValuePair<int, int>, List<int>> dupilicate_code_list = new();
        private void Button_Click(object sender, RoutedEventArgs e)
        {   // DO THE THING
            dupilicate_code_list.Clear();
            string[] code = textcontext.Text.Split(Environment.NewLine);
            for (int i = 0; i < code.Length; i++) // fixup the lines to all be nice and flat 
            {
                // while we're at it, remove the comments if there are any
                string code_line = code[i];
                int consectutive_dash = 0;
                for (int c_index = 0; c_index < code_line.Length; c_index++)
                {
                    char c = code_line[c_index];
                    if (c == '-')
                    {
                        consectutive_dash++;
                        if (consectutive_dash >= 2)
                        {
                            var v = code_line.Substring(0, c_index - 1);
                            code_line = v;
                        }
                    }
                    else
                    {
                        consectutive_dash = 0;
                    }
                }


                code[i] = code_line.Trim();
            }
            console_out.Children.Clear();


            for (int i = 0; i<code.Length; i++)
            {
                string line = code[i];

                var i_type = get_line_type(line);
                if (i_type == line_type.action || i_type == line_type.block) // is valid to attempt to optimize
                {
                    //if (i_type == line_type.block)
                    //{
                    //    relative_block_depth++;
                    //}
                    // run check across all lines of code to look for matches
                    for (int b = i+1; b < code.Length; b++)
                    {
                        if (b != i) // we still need a system so they dont read over each other maybe?, else a continuous segment of 2 alternating lines of code would be impossible to read beyond
                        {
                            var b_type = get_line_type(line);
                            if (b_type == line_type.action || b_type == line_type.block) // is valid to attempt to optimize
                            {

                                bool is_matching = true;
                                bool true_match = false;
                                int search_length = 2;
                                while (is_matching)
                                {
                                    if (code.Length < (i+search_length) || code.Length < (b + search_length)) // if either of the patterns has reached the end of the file
                                        break;

                                    var our_lines = code.Skip(i).Take(search_length).ToArray();
                                    var current_lines = code.Skip(b).Take(search_length).ToArray();
                                    if (current_lines.SequenceEqual(our_lines))
                                    {
                                        // make sure the blocks & their ends are accounted for when optimizing
                                        int relative_block_depth = 0;
                                        for (int benis = 0; benis < current_lines.Length; benis++)
                                        {
                                            var c_type = get_line_type(current_lines[benis]);
                                            if (c_type == line_type.block)
                                            {
                                                relative_block_depth++;
                                            }
                                            else if (c_type == line_type.end)
                                            {
                                                relative_block_depth--;
                                            }
                                        }
                                        if (relative_block_depth < 0)
                                            break;
                                        if (relative_block_depth == 0) // then theres a net balance and we're correctly checking the ends
                                        {
                                            true_match = true;
                                        }
                                        else
                                        {
                                            true_match = false;
                                        }
                                        search_length++;
                                    }
                                    else
                                    {
                                        is_matching = false;
                                    }
                                }
                                if (true_match)
                                {
                                    search_length--; // subtract it back one
                                    //string location_text = "matching code found at lines: " + (i+1) + "-" + (i+search_length) + " and " + (b+1) + "-" + (b+search_length) + " (match length:" + search_length + ")" + Environment.NewLine;

                                    //Button console_log = new Button();
                                    //console_log.Content = location_text;
                                    //console_log.Click += console_click;

                                    //console_log.Tag = new fard_structure(i,b,search_length);
                                    //console_out.Children.Add(console_log);
                                    var test = new KeyValuePair<int, int>(i, search_length);
                                    if (dupilicate_code_list.ContainsKey(test))
                                    {
                                        dupilicate_code_list[test].Add(b);
                                    }
                                    else
                                    {
                                        dupilicate_code_list.Add(test, new List<int>());
                                        dupilicate_code_list[test].Add(b);
                                    }
                                }
                            }
                        }
                    }

                }




            }
            // post optimization cleanups
            // if only has a single 2 liner match, then take it off the list
            for (int i=0; i < dupilicate_code_list.Count; i++)
            {
                var venis = dupilicate_code_list.ElementAt(i);
                if (venis.Value.Count == 1)
                {
                    if (venis.Key.Value == 2)
                    {
                        dupilicate_code_list.Remove(venis.Key);
                        i--;
                    }
                }
            }
            // cleanup optimizations that are overshadowed by other optimizations
            for (int i = 0; i < dupilicate_code_list.Count; i++)
            {

                var ienis = dupilicate_code_list.ElementAt(i);

                for (int b = 0; b < dupilicate_code_list.Count; b++)
                {
                    var benis = dupilicate_code_list.ElementAt(b);

                    if (i != b) // if i.line != b.line
                    {   // then they share the same start index
                        // test for collisions with any function, and determine whether  
                        int collisions = 0;
                        if (ienis.Key.Key <= benis.Key.Key && (ienis.Key.Key + ienis.Key.Value) >= (benis.Key.Key + benis.Key.Value))
                        {
                            collisions++;
                        }
                        foreach (var something in ienis.Value)
                        {
                            foreach (var otherthing in benis.Value)
                            {
                                if (something <= otherthing && (something + ienis.Key.Value) >= (otherthing + benis.Key.Value))
                                {
                                    collisions++;
                                }
                            }
                        }


                        if (collisions > 0)
                        {
                            int i_optimized_weight = (ienis.Key.Value * ienis.Value.Count) - ienis.Value.Count;

                            int b_optimized_weight = (benis.Key.Value * benis.Value.Count) - benis.Value.Count;

                            if (i_optimized_weight > b_optimized_weight)
                            {   // then our optimization that we're comparing (i) IS more efficient than the one we're currently looking at (b)
                                dupilicate_code_list.Remove(benis.Key);
                                b--;
                            }
                            else
                            {
                                dupilicate_code_list.Remove(ienis.Key);
                                i--;
                                break; // this should make it move to the next one, as we found a better optimization here
                            }
                        }
                    }


                }
            }

            // export the optimizations to functions
            int offset = 0;
            code = textcontext.Text.Split(Environment.NewLine);
            List<string> extracted_funcs = new();

            for (int i = 0; i < dupilicate_code_list.Count; i++)
            {
                var ienis = dupilicate_code_list.ElementAt(i);
                string poopy = "Autogenerated_F_" + ienis.Key.Key + "()";
                extracted_funcs.Add(convert_info_to_funct("function "+poopy, code.Skip(ienis.Key.Key).Take(ienis.Key.Value).ToArray()));
                // now we fixup this line to match our optimized function
                for (int p = ienis.Key.Key; p < ienis.Key.Key + ienis.Key.Value; p++)
                {
                    if (p == ienis.Key.Key) // is the first one
                    {
                        code[p] = new string(' ', count_pre_spaces(code[p])) + poopy;
                    }
                    else
                    {
                        code[p] = new string(' ', count_pre_spaces(code[p])) + "-- auto generated by RVTOOL";
                    }
                }

                for (int w = 0; w < ienis.Value.Count; w++)
                {
                    for (int p = ienis.Value[w]; p < ienis.Value[w] + ienis.Key.Value; p++)
                    {
                        if (p == ienis.Value[w]) // is the first one
                        {
                            code[p] = new string(' ', count_pre_spaces(code[p])) + poopy;
                        }
                        else
                        {
                            code[p] = new string(' ', count_pre_spaces(code[p])) + "-- auto generated by RVTOOL";
                        }
                    }
                }


            }
            string sausage = poo_join(code);
            for (int i = extracted_funcs.Count-1; i >= 0 ; i--)
            {
                sausage = extracted_funcs[i] + Environment.NewLine + sausage;
            }

            // tally up total optimizations
            int total_optimized_lines = 0;
            for (int i = 0; i < dupilicate_code_list.Count; i++)
            {
                var ienis = dupilicate_code_list.ElementAt(i);
                total_optimized_lines += (ienis.Key.Value * ienis.Value.Count) - ienis.Value.Count;
            }

            textcontext.Text = sausage;
        }
        public int count_pre_spaces(string s)
        {
            if (s.Length == 0) return 0;
            int i = 0;
            while (s[i] == ' ')
            {
                i++;
                if (i == s.Length) return 0;
            }
            return i;
        }
        public string convert_info_to_funct(string line, string[] code)
        {
            string output = line;
            // clip spaces to be +3 of the relative indentation of the first line of code
            int i = count_pre_spaces(code[0]);
            for (int index = 0; index < code.Length; index++) code[index] = (code[index].Length >= i) ? "   " + code[index].Substring(i): code[index];

            return output + Environment.NewLine + poo_join(code) + Environment.NewLine + "end";
        }

        public struct fard_structure
        {
            public fard_structure(int A, int B, int C)
            {
                first_occurance = A;
                last_occurance = B;
                occurance_length = C;
            }
            public int first_occurance;
            public int last_occurance;
            public int occurance_length;
        }
        public bool toggle_focus = false;
        public void console_click(object sender, RoutedEventArgs e)
        {

            Button? butt = sender as Button;
            fard_structure output_info = (fard_structure)(butt.Tag as fard_structure?);
            // if we managed to get past that then
            int x;
            if (toggle_focus)
            {
                x = output_info.first_occurance;
            }
            else
            {
                x = output_info.last_occurance;
            }
            string[] code = textcontext.Text.Split(Environment.NewLine);
            int chars_past = 0;
            for (int i = 0; i < x; i++)
            {
                chars_past += code[i].Length;
                chars_past++; // to account for the newline added
                chars_past++; // to account for the space inbetween the last iteration
            }

            int chars_selected = 0;
            for (int i = 0; i < output_info.occurance_length; i++)
            {
                chars_selected += code[x+i].Length;
                chars_selected++; // to account for the lines between
                chars_selected++; // to account for the new lines between 
            }

            textcontext.SelectionStart = chars_past;
            textcontext.SelectionLength = chars_selected;
            textcontext.Focus();

            toggle_focus = !toggle_focus;
        }

        enum line_type
        {
            block,
            action,
            end,
            impossible,
            none
        }
        line_type get_line_type(string line)
        {
            string block_check__2 = (line.Length >= 2) ? line.Substring(0, 2): ""; // i cant remember the better way of doing this 
            string block_check__3 = (line.Length >= 3) ? line.Substring(0, 3): ""; // i cant remember the better way of doing this 
            string block_check__4 = (line.Length >= 4) ? line.Substring(0, 4): ""; // i cant remember the better way of doing this 
            string block_check__6 = (line.Length >= 6) ? line.Substring(0, 6): ""; // i cant remember the better way of doing this 
            string block_check__8 = (line.Length >= 8) ? line.Substring(0, 8) : ""; // i cant remember the better way of doing this 
            string block_check__9 = (line.Length >= 9) ? line.Substring(0, 9): ""; // i cant remember the better way of doing this 

            // declare 
            // enum

            if (block_check__3 == "if " || block_check__4 == "alt " || block_check__6 == "altif " || block_check__3 == "do " || block_check__2 == "do" || block_check__4 == "for ") // block start
            {
                return line_type.block;
            }
            else if (block_check__3 == "on " || block_check__9 == "function ") // this line cannot be optimized
            {
                return line_type.impossible;
            }
            else if (block_check__4 == "end " || block_check__3 == "end") // skip but decrease depth by one
            {
                return line_type.end;
            }
            else if (block_check__6 == "alias " || block_check__8 == "declare " || line == "" || block_check__2 == "--") // skip, none of these are code
            {
                return line_type.none;
            }
            else // finally, just a regular line of code
            {
                return line_type.action;
            }
        }

        private void textcontext_TextChanged(object sender, TextChangedEventArgs e)
        {   // make sure the line counter has the appropriate amount of lines
            if (linecontext == null)
                return;
            string[] code = textcontext.Text.Split(Environment.NewLine); // theres probably a way better way to do this

            string[] lines = linecontext.Text.Split(Environment.NewLine); // theres probably a way better way to do this
            if (code.Length > lines.Length) // add more lines
            {
                for (int i = lines.Length; i < code.Length; i++)
                {
                    if (!string.IsNullOrEmpty(linecontext.Text))
                        linecontext.Text += Environment.NewLine;
                    linecontext.Text += (i + 1);
                }
            }
            else if (code.Length < lines.Length) // take away some lines
            {
                linecontext.Text = poo_join(lines.Take(code.Length).ToArray()); //.(Environment.NewLine);
            }
        }
        string poo_join(string[] string_array)
        {
            string output = "";
            for (int i = 0; i < string_array.Length; i++)
            {
                if (i > 0)
                    output += Environment.NewLine;
                output += string_array[i];
            }
            return output;
        }
    }
}
