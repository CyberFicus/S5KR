using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_RSC
{
    internal class Polynom
    {
        public byte[] coeffs { set; get; }
        public Field field { private set; get; }

        public bool field_is_correct { get { return field.is_correct;} }
        public bool is_ok { private set; get; }

        public int deg { get { 
                for (int i  = coeffs.Length - 1; i > 0; i--)
                {
                    if (coeffs[i] != 0) return i;
                }
                return 0;
            } }

        public Polynom(Field field, byte[]? init_arr, bool cut = false)
        {
            this.field = field;
            if (!field_is_correct || init_arr == null) { is_ok = false; coeffs = new byte[] { 0 }; return; }


            int len = init_arr.Length;
            // отбрасываем нулевые коэффициенты
            //*
            while (cut && len > 1 && init_arr[len-1] == 0) 
            {
                len--;
            } //*/ // может помешать при кодировании сообщения, обрезав старший коэф-т
            
            coeffs = new byte[len];
            for (int i = 0; i < len; i++)
            {
                coeffs[i] = (byte)(init_arr[i] & (field.q - 1));
            }
            is_ok = true;
        }

        public Polynom(Polynom p)
        {
            this.field = p.field;
            this.is_ok = p.is_ok;

            byte[] new_arr = new byte[p.deg + 1];
            for (int i = 0; i<new_arr.Length; i++)
            {
                new_arr[i] = (byte) ((field.q-1) & p.coeffs[i]);
            }

            this.coeffs = new_arr;
        }

        public string print(char? c = 'x')
        {
            string s = "";
            if (!is_ok) { return "Incorrect field or polynom!"; }
            for (int i = 0; i<= deg; i++)
            {
                var buf = field.to_string_pow(coeffs[i]);
                if (buf == "1" && i > 0) { buf = ""; }
                if (buf == "0") {
                    if (deg == 0) { return "0"; }
                    continue; 
                }
                switch (i)
                {
                    case 0: s += buf;
                        break;
                    case 1: s = buf + (c!=null ? c + ( s!="" ? " + " :"") : " ") + s;
                        break;
                    default: s = buf + (c != null ? $"{c}^{i}" + (s != "" ? " + " : "") : " ") + s;
                        break;
                }
            }
            return s;
        }

        public string ToString()
        {
            return this.print();
        }

        public Polynom add(Polynom p)
        {
            if (this.is_ok == false || p.is_ok == false) { return new Polynom(field, null); }

            int len = (this.coeffs.Length >= p.coeffs.Length) ? this.coeffs.Length : p.coeffs.Length;
            byte[] new_arr = new byte[len];

            for (int i = 0; i < len; i++)
            {
                new_arr[i] = (byte) ( (i <= this.deg ? coeffs[i] : 0) ^ (i <= p.deg ? p.coeffs[i] : 0));
            }

            return new Polynom(field, new_arr);
        }
        
        public Polynom mul(Polynom p)
        {
            if (this.is_ok == false || p.is_ok == false) { return new Polynom(field, null); }

            int len = 1 + this.deg + p.deg;
            byte[] new_arr = new byte[len];

            for (int i = 0; i < len; i++)
            {
                int j = 0, k = i - j; // j - итератор для первого полинома, k - для второго
                byte buf;

                while (k >= 0) {
                    if (j < coeffs.Length && k < p.coeffs.Length)
                    {
                        buf = field.mul(coeffs[j], p.coeffs[k]);
                        new_arr[i] = field.add(new_arr[i], buf);
                    }
                    j++;
                    k--;
                }
            }

            return new Polynom(field, new_arr);
        }

        public Polynom mul(byte a)
        {
            if (this.is_ok == false) { return new Polynom(field, null); }

            int len = 1 + this.deg;
            byte[] new_arr = new byte[len];

            for (int i = 0; i < len; i++)
            {
                new_arr[i] = field.mul(this.coeffs[i], a);
            }

            return new Polynom(field, new_arr);
        }

        public Polynom div(Polynom p)
        {
            // f(x) / p(x)
            // f(x) = p(x)*q(x) + r(x)
            // возвращаем q(x) - неполное частное
            // и r(x) - остаток

            if (this.field_is_correct == false || p.field_is_correct == false) { return new Polynom(field, new byte[] { 0 }); }
            // массив для результата деления 
            int d = this.deg - p.deg; // разницы в степенях
            // deg q(x) = d
            byte[] arr_q= new byte[d+1];

            Polynom f = new Polynom(this);

            for (int i = d; i >= 0; i--)
            {
                if (f.coeffs[i + p.deg] == 0) { arr_q[i] = 0; continue; }

                byte c = field.div(f.coeffs[f.deg], p.coeffs[p.deg]);
                arr_q[i] = c; 
                Polynom temp = p.mul(c);
                
                for (int j = temp.deg; j >=0; j--)
                {
                    f.coeffs[j + i] = field.add(f.coeffs[j + i], temp.coeffs[j]);
                }

            }

            return new Polynom(field, arr_q);
        }

        public Polynom div(Polynom p, out Polynom r)
        {
            // f(x) / p(x)
            // f(x) = p(x)*q(x) + r(x)
            // возвращаем q(x) - неполное частное
            // и r(x) - остаток

           
            if (!this.is_ok || !p.is_ok || (p.deg == 0 && p.coeffs[0] == 0) ) { r = new Polynom(field, null); return new Polynom(field, new byte[] { 0 }); }
            
            if (deg == 0 && coeffs[0] == 0)
            {
                r = new Polynom(this);
                return new Polynom(this);
            }
            
            // массив для результата деления 
            int d = this.deg - p.deg; // разницы в степенях
            // deg q(x) = d
            byte[] arr_q = new byte[d + 1];

            Polynom f = new Polynom(this);

            for (int i = d; i >= 0; i--)
            {
                if (f.coeffs[i + p.deg] == 0) { arr_q[i] = 0; continue; }

                byte c = field.div(f.coeffs[f.deg], p.coeffs[p.deg]);
                arr_q[i] = c;
                Polynom temp = p.mul(c);

                for (int j = temp.deg; j >= 0; j--)
                {
                    f.coeffs[j + i] = field.add(f.coeffs[j + i], temp.coeffs[j]);
                }

            }
            r = new Polynom(field, f.coeffs);
            return new Polynom(field, arr_q);
        }
        public Polynom norm()
        {
            if (is_ok)
            {
                byte c = field.div(1, coeffs[deg]);
                coeffs[deg] = 1;
                for (int i = deg - 1; i >= 0; i--)
                {
                    coeffs[i] = field.mul(coeffs[i], c);
                }
            }
            return new Polynom(field, coeffs);
        }

        public Polynom gcd(Polynom p)
        {
            if (this.field_is_correct == false || p.field_is_correct == false) { return new Polynom(field, null); }
            if (this.deg == 0 ||  p.deg == 0) { return new Polynom(field, new byte[] { 1 }); }

            Polynom r_prev2, r_prev1;

            if (deg >= p.deg)
            {
                r_prev2 = new Polynom(this);
                r_prev1 = new Polynom(p);
            }
            else {
                r_prev2 = new Polynom(p);
                r_prev1 = new Polynom(this);
            }
            // a0, b0, a1, b1
            Polynom a_prev2 = new Polynom(field, new byte[] { 1 });
            Polynom b_prev2 = new Polynom(field, new byte[] { 0 });
            Polynom a_prev1 = new Polynom(field, new byte[] { 0 });
            Polynom b_prev1 = new Polynom(field, new byte[] { 1 });

            Polynom r_cur, q_cur, a_cur, b_cur;

            while (true) { 
                q_cur = r_prev2.div(r_prev1, out r_cur);

                if (r_cur.deg == 0 && r_cur.coeffs[0] == 0)
                {
                    break;
                }

                a_cur = a_prev2.add(q_cur.mul(a_prev1));
                b_cur = b_prev2.add(q_cur.mul(b_prev1));

                a_prev2 = a_prev1; a_prev1 = a_cur;
                b_prev2 = b_prev1; b_prev1 = b_cur;
                r_prev2 = r_prev1; r_prev1 = r_cur;
            }
            return r_prev1.norm();
        }

        // f(x) при x = ...
        public byte sub(byte x)
        {
            if (this.is_ok == false) { return 0; }

            byte res = coeffs[deg];
            for (int i = this.deg; i > 0; i--) {
                res = field.mul(res, x);
                res = field.add(res, coeffs[i-1]);
            }

            return res;
        }

        public static Polynom by_roots(Field f, byte[] roots_poly)
        {
            if (roots_poly.Length == 0)
            {
                return new Polynom(f, null);
            }

            var res = new Polynom(f, new byte[] {1});
            for (int i = 0; i < roots_poly.Length; i++)
            {
                var temp = new Polynom(f, new byte[] { roots_poly[i], 1 } );
                res = res.mul(temp);
            }

            return res;
        }

        public string ToBitString()
        {
            if (!is_ok) { return ""; }

            var res = "";

            for (int i = coeffs.Length-1; i >=0; i--)
            {
                res += field.to_string_poly(coeffs[i]) + " ";
            }

            return res;
        }

        public Polynom shift(int s )
        {
            if (!is_ok) { return new Polynom(this.field, null); }
            if (-s > deg) { return new Polynom(this.field, new byte[] { 0 }); }

            var new_arr = new byte[coeffs.Length + s];
            for (int i = deg+s; (i-s >= 0 && i >=0); i--)
            {
                new_arr[i] = coeffs[i-s];
            }

            return new Polynom(this.field, new_arr, false);
        }

        // метод Ченя

        public byte[] getRevRoots()
        {
            if (!is_ok) { return new byte[0]; }

            var list = new List<byte>();
            for (int i = 0; i < field.q-1; i++)
            {
                var rev = field.rev(field.pow_to_poly[i]);

                if (this.sub(field.pow_to_poly[i]) == 0)
                {
                    list.Add(rev);
                }
            }

            return list.ToArray();
        }

        public byte[] getRoots()
        {
            if (!is_ok) { return new byte[0]; }

            var list = new List<byte>();
            for (int i = 0; i < field.q - 1; i++)
            {
                if (this.sub(field.pow_to_poly[i]) == 0)
                {
                    list.Add(field.pow_to_poly[i]);
                }
            }

            return list.ToArray();
        }


        // возвращает lambda(X) = sigma(x)*S(x) и sigma(x) чреез out
        // принимает: gen -> определяет модуль, по которому делим, x^{2 gen.deg +1}; S(x)  
        public static Polynom gcd_RS( Polynom gen, Polynom S, out Polynom b_sigma)
        {   
            Polynom p = new Polynom(gen.field, new byte[] { 1 }); // x^{2 gen.deg + 1}
            p = p.shift(gen.deg + 1);
            // добавить код на случай нулевых степеней и прочей говнины
            if (S.field_is_correct == false || p.field_is_correct == false) { b_sigma = new Polynom(gen.field, new byte[] { 0 }); return new Polynom(b_sigma); }

            Polynom r_prev2, r_prev1;

            if (S.deg >= p.deg)
            {
                r_prev2 = new Polynom(S);
                r_prev1 = new Polynom(p);
            }
            else
            {
                r_prev2 = new Polynom(p);
                r_prev1 = new Polynom(S);
            }
            // a0, b0, a1, b1
            Polynom a_prev2 = new Polynom(gen.field, new byte[] { 1 });
            Polynom b_prev2 = new Polynom(gen.field, new byte[] { 0 });
            Polynom a_prev1 = new Polynom(gen.field, new byte[] { 0 });
            Polynom b_prev1 = new Polynom(gen.field, new byte[] { 1 });

            Polynom r_cur, q_cur, a_cur, b_cur;

            while (true)
            {
                q_cur = r_prev2.div(r_prev1, out r_cur);

                a_cur = a_prev2.add(q_cur.mul(a_prev1));
                b_cur = b_prev2.add(q_cur.mul(b_prev1));
               
                if (r_cur.deg <= (gen.deg / 2))
                {
                    b_sigma = b_cur;
                    return r_cur;
                }

                a_prev2 = a_prev1; a_prev1 = a_cur;
                b_prev2 = b_prev1; b_prev1 = b_cur;
                r_prev2 = r_prev1; r_prev1 = r_cur;
            }



        }

        public Polynom deriv()
        {
            if (!is_ok) { return new Polynom(this.field, null); }

            var s = this.shift(-1);
            for (int i = 1; i < s.coeffs.Length; i += 2) {
                s.coeffs[i] = 0;
            }

            return s;
        }
    }
}
