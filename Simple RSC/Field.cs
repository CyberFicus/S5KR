using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple_RSC
{
    internal class Field
    {
        public short gen { private set; get; }
        public int m; // степень расширения GF(2^m)
        public int q; // q = 2^m
        public bool is_correct = false;

        public byte[] pow_to_poly { private set; get; } // q-1 уникальных элементов
        public byte?[] poly_to_pow { private set; get; } // q элементов

        public Field(int gen)
        {
            q = 1;
            m = 0; // длина битового полиномиального представления элемента поля - m бит 
            gen = gen & 0x1ff; // не более 8 степени
            while (q < gen)
            {
                q = (q << 1);
                m++;
            }
            q = q >> 1;
            m--;
            // таблицы степень-полином и полином-степень
            pow_to_poly = new byte[q]; pow_to_poly[0] = 1;
            poly_to_pow = new byte?[q]; poly_to_pow[1] = 0;
            int buf = 1;
            //заполнение таблиц с проверкой на преждевременное замыкание
            for (byte i = 1; i < q - 1; i++)
            {
                buf = buf << 1;
                
                if ((buf & q) != 0)
                {
                    buf = gen ^ buf;
                }
                pow_to_poly[i] = (byte) buf;

                if (buf == 1)
                {
                    return;
                }

                poly_to_pow[buf] = i;
            }
            //проверка на замыкание a^q-1 = 0
            buf = buf << 1;
            if ((buf & q) != 0)
            {
                buf = gen ^ buf;
            }
            if (buf != 1)
            {
                return;
            }
            pow_to_poly[q-1] = (byte)buf;
            is_correct = true;
        }

        public static Field newByStr(string str)
        {
            int gen = 0;
            for (int i = 0; i < str.Length; i++)
            {
                var buf = int.Parse("" + str[i]);
                if (buf >= 0 && buf <= 8)
                {
                    gen = gen | (int)Math.Pow(2, buf);
                }
            }

            return new Field(gen);
        }

        public string to_string_poly(byte input)
        {
            var p = input & (q - 1);

            var str = Convert.ToString(p, 2);
            while (str.Length < m)
            {
                str = "0" + str;
            }
            return str;
        }

        public string to_string_pow(byte input, char c = 'a')
        {
            var buf = input & (q - 1);

            byte? pow = poly_to_pow[buf];

            switch (pow)
            {
                case null:
                    return "0";
                case 0:
                    return "1";
                default:
                    return $"{c}^{pow}";
            }
        }

        public string ToString()
        {
            if (!is_correct)
            {
                return "Поле некорректно";
            }

            var str = ""; 
            var buf = "  ";
            for (int i = 0; i < pow_to_poly.Length; i++)
            {
                str += $"a^{i}" + buf + " = " + this.to_string_poly(pow_to_poly[i]) + "  |  " + this.to_string_poly((byte)i) + " = " + i.ToString() + " = " + this.to_string_pow((byte)i) + "\n";

                switch ( i )
                {
                    case 9:
                        buf = " ";
                        break;
                    case 99:
                        buf = "";
                        break;
                }
            }
            return str;
        }

        // операции
        public byte add(byte a, byte b)
        {
            return (byte) ( (q - 1) & (a ^ b) );
        }

        public byte mul(byte a, byte b)
        {
            a = (byte)(a & (q - 1));
            b = (byte)(b & (q - 1));

            if (a == 0 || b == 0 || is_correct == false) { return 0; }

            byte a_pow = poly_to_pow[a].Value; // не может вернуть null
            byte b_pow = poly_to_pow[b].Value; 

            int res_pow = (a_pow + b_pow) % (q-1);
            
            return pow_to_poly[res_pow];
        }

        public byte div(byte a, byte b)
        {
            a = (byte) (a & (q - 1));
            b = (byte) (b & (q - 1));

            if (a == 0 || b == 0 || is_correct == false) { return 0; }

            byte a_pow = poly_to_pow[a].Value; // не может вернуть null
            byte b_pow = poly_to_pow[b].Value;

            int res_pow = (a_pow - b_pow) % (q - 1);
            if (res_pow < 0) { res_pow += q - 1; }

            return pow_to_poly[res_pow];
        }

        public byte rev(byte a) // нахождение обратного элемента
        {
            a = (byte) (a & (q -1) );
            if (a == 0 || !is_correct) { return 0; }

            byte a_pow = poly_to_pow[a].Value; // не может вернуть null
            a_pow = (byte) (q - 1 - a_pow);
            return pow_to_poly[a_pow];
        }

        public byte pow(byte a, int p) // возведение в степень p
        {
            a = (byte)(a & (q - 1));
            p = p & (q-1);
            if (p < 0) { p += q - 1; }

            if (is_correct == false || a == 0 || p == 0) { return 0; }
            
            byte a_pow = poly_to_pow[a].Value; // не может вернуть null
            return pow_to_poly[ (a_pow * p) % (q-1) ];
        }
    }
}