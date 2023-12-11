namespace Simple_RSC
{
    class RSC
    {
        RS_form sourceform; 

        public RSC(RS_form src)
        {
            sourceform = src;
        }

        private Polynom Generate(string gen_str, int b_pow, int td, out byte[] code_generator_roots)
        {
            Field f;
            try { 
                f = Field.newByStr(gen_str);
            } 
            catch (System.Exception ex)
            {
                sourceform.logexc(ex);
                code_generator_roots = new byte[0]; 
                return new Polynom( Field.newByStr("310"), null); // чтобы вернуть хоть что-то
            }
            
            if (!f.is_correct)
            {
                code_generator_roots = new byte[0]; 
                return new Polynom(f, null);
            }

            if (b_pow < 0)
            {
                b_pow = (b_pow % f.q-1) + f.q;
            }

            byte[] arr = new byte[2 * td];
            for (int i = 0; i < 2 * td; i++)
            {
                arr[i] = (f.pow_to_poly[(b_pow + i) % (f.q - 1)]);
            }

            code_generator_roots = arr;
            return Polynom.by_roots(f, arr);
        }

        private Polynom NewMessage(Polynom gen, out Polynom code_word ){
            var f = gen.field;
            var n = f.q - 1 - gen.deg;
            var arr = new byte[n];
            var rnd = new Random();

            for (int i = 0; i < n; i++)
            {
                arr[i] = (byte)rnd.Next(0, f.q);
            }

            var message = new Polynom(f, arr);

            var message_shifted = message.shift(gen.deg);
            Polynom r;
            message_shifted.div(gen, out r);
            code_word = message_shifted.add(r);

            return message;
        }

        private Polynom AddErrors(Polynom code_word, int n, out Polynom error_true)
        {
            var f = code_word.field;

            if (n < 0) { n = -n; }
            if (n > f.q-1) { n = f.q - 1; }

            var arr_e = new byte[f.q-1];
            var rnd = new Random();

            var j = rnd.Next(0, f.q - 1);

            for (int i = 0; i< n;  i++)
            {
                while (arr_e[j] != 0) {
                    j = rnd.Next(0, f.q - 1);
                }
                arr_e[j] = (byte) rnd.Next(1, f.q);
            }

            error_true = new Polynom(f, arr_e);
            
            if ( n == 0)
            {
                return code_word;
            }

            var recieved = code_word.add(error_true);
            return recieved;
        }

        private Polynom Check(Polynom recieved, byte[] code_generator_roots, out bool error_flag)
        {
            error_flag = false;
            var arr_S = new byte[code_generator_roots.Length + 1];
            arr_S[0] = 1;

            for (int i = 0; i < code_generator_roots.Length; i++)
            {
                arr_S[i + 1] = recieved.sub(code_generator_roots[i]);
                if (arr_S[i + 1] != 0) { error_flag = true; }
            }
            var syndromes = new Polynom(recieved.field, arr_S);

            return syndromes;
        }

        private void EucledesExtended(Polynom syndromes, Polynom code_generator, out Polynom sigma, out Polynom lambda) {
            var f = syndromes.field;
            Polynom mod = new Polynom(f, new byte[] { 1 });
            mod = mod.shift(code_generator.deg + 1);

            Polynom r_prev2, r_prev1;

            if (syndromes.deg >= mod.deg)
            {
                r_prev2 = new Polynom(syndromes);
                r_prev1 = new Polynom(mod);
            }
            else
            {
                r_prev2 = new Polynom(mod);
                r_prev1 = new Polynom(syndromes);
            }
            Polynom a_prev2 = new Polynom(f, new byte[] { 1 });
            Polynom b_prev2 = new Polynom(f, new byte[] { 0 });
            Polynom a_prev1 = new Polynom(f, new byte[] { 0 });
            Polynom b_prev1 = new Polynom(f, new byte[] { 1 });

            Polynom r_cur, q_cur, a_cur, b_cur;

            while (true)
            {
                q_cur = r_prev2.div(r_prev1, out r_cur);

                a_cur = a_prev2.add(q_cur.mul(a_prev1));
                b_cur = b_prev2.add(q_cur.mul(b_prev1));

                if (r_cur.deg <= (code_generator.deg / 2))
                {
                    sigma = b_cur;
                    lambda = r_cur;
                    return;
                }

                a_prev2 = a_prev1; a_prev1 = a_cur;
                b_prev2 = b_prev1; b_prev1 = b_cur;
                r_prev2 = r_prev1; r_prev1 = r_cur;
            }
        }

        private Polynom CalculateError(Polynom sigma, Polynom lambda, int b_pow, out byte[] locators)
        {
            // сигма - локаторы, лямбда - ошибки
            var f = sigma.field;
            b_pow = b_pow & (f.q-1);
            locators = sigma.getRoots();
            byte[] arr_e = new byte[f.q - 1];

            for (int i = 0; i < locators.Length; i++)
            {
                var val = locators[i];
                var rev_r = f.rev(val);

                var l_sub = lambda.sub(val);
                var s_sub = sigma.deriv().sub(val);

                var pow  =  (2 - b_pow) % (f.q-1);

                var magn = f.pow(rev_r, pow);
                magn = f.mul(magn, l_sub);
                magn = f.div(magn, s_sub);

                arr_e[(byte)f.poly_to_pow[rev_r]] = magn;

                locators[i] = rev_r;
            }

            return new Polynom(f, arr_e);
        }

        private Polynom Decode(Polynom recieved, Polynom? errors, Polynom code_generator, out Polynom amended)
        {
            if (errors != null) { 
                amended = recieved.add(errors);
            } else
            {
                amended = new Polynom(recieved);
            }
            return amended.shift(-code_generator.deg);
        }

        public void Output(string str)
        {
            sourceform.write(str);
        }

        public void Run(string gen_str, int b, int td, int err)
        {
            sourceform.clear();

            if (td < 1)
            {
                Output("Считанное значение корректирующей способности кода меньше 1. Изменено на 1.\n\n");
                td = 1;
            }

            if (err < 0)
            {
                Output("Считанное кол-во генерируемых ошибок меньше 0. Изменено на 0.\n\n");
                err = 0;
            }

            Output(
                $"НАЧАЛО РАССЧЁТОВ\n\n" +
                "Считанные параметры:\n" +
                $"Строковое представление порождающего многочлена поля Галуа: {gen_str}\n" +
                $"Степень первого корня порождающего многочлена кода: {b}\n" +
                $"Количество корректируемых ошибок (td): {td}\n" +
                $"Количество генерируемых символьных ошибок: {err}\n\n"
             );

            byte[] code_generator_roots;
            Polynom code_generator = Generate(gen_str, b, td, out code_generator_roots);

            if (code_generator_roots.Length == 0)
            {
                Output(
                    "Ошибка: не удалось рассчитать поле Галуа или порождающий многочлен кода. Убедитесь, что введён корректный порождающий многочлен поля.\n" + 
                    "Рекомендуемые порождающие многочлены поля:\n - 310\n - 410\n - 520\n - 610\n - 710\n - 84320"
                );
                sourceform.focus();
                return;
            }

            if (code_generator.field.q < 8)
            {
                Output(
                    "Ошибка: размер поля Галуа слишком мал. Убедитесь, что введён корректный порождающий многочлен поля.\n" +
                    "Рекомендуемые порождающие многочлены поля:\n" + 
                    " -- 310 - для GF(2^3)\n -- 410 - для GF(2^4)\n -- 520 - для GF(2^5)\n -- 610 для GF(2^6)\n -- 710 для GF(2^7)\n -- 84320 - для GF(2^8)"
                );
                sourceform.focus();
                return;
            }

            if (code_generator.field.q - 1 <= 2*err )
            {
                td = (code_generator.field.q - 2 ) /2;
                Output(
                    "Ошибка: слишком большое количество исправляемых ошибок, в кодовом слове на хватит места для передаваемой информации.\n" +
                    $"Новое значение td: {td}\n\n"
                );
                sourceform.focus();
                code_generator = Generate(gen_str, b, td, out code_generator_roots);
            }

            var roots_string = "";
            for (int i = 0; i < code_generator_roots.Length; i++)
            {
                roots_string += code_generator.field.to_string_pow(code_generator_roots[i]) + " ";
            }

            Output(
                "Корни порождающего многочлена:\n" + roots_string + "\n" +
                "Порождающий многочлен кода:\n" + code_generator.ToString() + "\n" +
                "Двоичное представление:\n" + code_generator.ToBitString() + "\n\n"
            );

            Polynom codeword, message;
            try
            {
                message = NewMessage(code_generator, out codeword);
            }
            catch (System.Exception ex)
            {
                Output("Ошибка: не удалось закодировать сообщение. Попробуйте ввести другие исходные данные.");
                sourceform.focus();
                sourceform.logexc(ex);
                return;
            }

            Output(
                "Cгенерированное исходное сообщение:\n" + message.ToBitString() + "\n" +
                "Полиномиальная форма:\n" + message.ToString() + "\n\n" +
                "Полученное кодовое слово:\n" + codeword.ToBitString() + "\n" +
                "Полиномиальная форма:\n" + codeword.ToString() + "\n\n"
            );

            if (err > code_generator.field.q - 1)
            {
                err = code_generator.field.q - 1;
                Output($"Предупреждение: количество генерируемых ошибок превышеает длину кодового слова. Количество генерируемых ошибок уменьшено до {err}\n");
            }

            Polynom errors_true, recieved;
            try
            {
                recieved = AddErrors(codeword, err, out errors_true);
            }
            catch (System.Exception ex)
            {
                Output("Ошибка: не удалось сгенерировать многочлен ошибок. Попробуйте изменить количество генерируемых ошибок");
                sourceform.focus();
                sourceform.logexc(ex);
                return;
            }

            Output(
                "Сгенерированные ошибки передачи:\n" + errors_true.ToBitString() + "\n" +
                "Полиномиальная форма:\n" + errors_true.ToString() + "\n\n" +
                "Сообщение, полученное после передачи по зашумлённому каналу:\n" + recieved.ToBitString() + "\n" +
                "Полиномиальная форма:\n" + recieved.ToString() + "\n\n" +
                "Сравнение:\n" +
                "Передано: " + codeword.ToBitString() + "\n" +
                "Получено: " + recieved.ToBitString() + "\n\n"
            );


            bool error_flag;
            Polynom syndromes;
            try
            {
                syndromes = Check(recieved, code_generator_roots, out error_flag);
            }
            catch (System.Exception ex)
            {
                Output("Ошибка: не удалось рассчитать многочлен синдромов. Попробуйте изменить входные данные");
                sourceform.focus();
                sourceform.logexc(ex);
                return;
            }

            Output(
                "Вычислим многочлен синдромов, поочерёдно подставляя в полиномиальную форму полученного сообщения корни порождающего многочлена кода:\n" + 
                "Полиномиальная форма:\n"+ syndromes.ToString() + "\n" +
                "Двоичная форма:\n" + syndromes.ToBitString() + "\n\n"
            );

            if (!error_flag)
            {
                Output("Поскольку все синдромы равны нулю, на этом моменте можно заканчивать: ошибок при передаче не было, декодирование элементарное.\nНо мы всё же посмотрим, что будет дальше.\n\n");
            }


            Output(
                "Обозначим многочлен синдромов как S(x). С его помощью мы найдём два других важных многочлена: L(x) и M(x)\n" +
                "L(x) - это многочлен локаторов ошибок, его корни позволяют определить, какие именно коэффициенты многочлена ошибок следует вычислять\n" +
                "M(x) - это многочлен значений ошибок, с его помщью мы вычислим коэффициенты многочлена ошибок\n" + 
                "Эти многочлены связаны следующим соотношением: M(x) = L(x)S(x) mod x^{2*td + 1}\n" +
                "С помощью Расширенного Алгоритма Евклида для многочленов мы можем вычислить линейное представление НОД S(x) и x^{2*td + 1}\n" +
                "На одном из шагов РАЕ мы непремено окажемся в следующей ситуации:\n r_i(x) = S(x) * a_i(x) + x^{2*td + 1} * b_i(x); где степень r_i(x) будет меньше 2*td+1\n" +
                "Тогда объявим r_i(x) = M(x) и a_i(x) = L(x), ведь для них выполняется условие M(x) = L(x)S(x) mod x^{2*td + 1}\n\n"
            );
            

            Polynom sigma, lambda;
            try
            {
                EucledesExtended(syndromes, code_generator, out sigma, out lambda);
            }
            catch (System.Exception ex)
            {
                Output("Ошибка: не удалось рассчитать многочлены локаторов и значений ошибок. Попробуйте изменить входные данные");
                sourceform.focus();
                sourceform.logexc(ex);
                return;
            }

            byte[] locators;
            Polynom errors_eval;
            try
            {
                errors_eval = CalculateError(sigma, lambda, b, out locators);
            }
            catch (System.Exception ex)
            {
                Output("Ошибка: не удалось рассчитать многочлен ошибок. Попробуйте изменить входные данные");
                sourceform.focus();
                sourceform.logexc(ex);
                return;
            }

            var locators_str = "";
            var roots_str = "";
            for (int i = 0; i < locators.Length; i++)
            {
                roots_str += code_generator.field.to_string_pow(locators[i]) + " ";
                locators_str += code_generator.field.poly_to_pow[locators[i]].ToString() + " ";
            }

            Output(
                "Многочлен локаторов ошибок L(x), найденный при помощи РАЕ:\n" + sigma.ToString() + "\n" +
                "Двоичное представление:\n" + sigma.ToBitString() + "\n" +
                "Корни многочлена локаторов ошибок:\n" + roots_str + "\n" +
                "Локаторы ошибок:\n" + locators_str + "\n\n"
            );

            Output(
                "Многочлен значений ошибок M(x), найденный при помощи РАЕ:\n" + lambda.ToString() + "\n" +
                "Двоичное представление:\n" + lambda.ToBitString() + "\n\n"
            );

            // сюда про вычисление многочлена ошибки по алгоритму форни
            var deriv = sigma.deriv();
            Output(
                "Многочлен значений ошибок вычисляется при помощи алгоритма Форни\n" +
                "Пройдёмся по массиву локаторов ошибок. Для каждого локатора j коэффициент многочлена ошибок при степени x^j находится по формуле^\n" +
                "(a^j)^{2-b} * M(a^-j) / L'(a^-j)\n" +
                "Где M(x) - многочлен значений ошибок, а L'(x) - формальная производная многочлена локаторов ошибок\n\n" +
                "В нашем случае L'(x) имеет следующий вид:\n" + deriv.ToString() +
                "Двоичное представление:\n" + deriv.ToBitString() + "\n\n"
            );


            Output(
                "Вычисленный с помощью алгоритма Форни многочлен ошибок:\n" + errors_eval.ToString() + "\n" +
                "Двоичное представление:\n" + errors_eval.ToBitString() + "\n\n"
            );


            Output(
                "Сравнение многочленов ошибок:\n" +
                "Настоящий:   " + errors_true.ToBitString() + "\n" +
                "Вычисленный: " + errors_eval.ToBitString() + "\n\n"
            );

            Polynom decoded, amended;
            try
            {
                decoded = Decode(recieved, errors_eval, code_generator, out amended);
            }
            catch (System.Exception ex)
            {
                Output("Ошибка: не удалось декодировать исправленное сообщение. Попробуйте изменить входные данные");
                sourceform.focus();
                sourceform.logexc(ex);
                return;
            }

            Output(
                "Принятое сообщение с исправленными ошибками:\n" + amended.ToBitString() + "\n" +
                "Полиномиальная форма:\n" + amended.ToString() + "\n\n" +
                "Сравнение кодовых слов:\n" +
                "Отправленное: " + codeword.ToBitString() + "\n" +
                "Исправленное: " + amended.ToBitString() + "\n\n"
            );

            Output(
                "Декодированное сообщение:\n" + decoded.ToBitString() + "\n" +
                "Полиномиальная форма:\n" + decoded.ToString() + "\n\n" +
                "Сравнение сообщений:\n" +
                "Отправленное: " + message.ToBitString() + "\n" +
                "Полученное:   " + decoded.ToBitString() + "\n\n"
            );

                Output(
                "=== === === === === \n\n" +
                "ТАБЛИЦЫ ПОЛЯ ГАЛУА:\n \"Степень -> Полином\" и \"Полином -> Степень\" \n\n" +
                code_generator.field.ToString()
            );
        }

        public void RunString(string str)
        {
            sourceform.clear();

            string[]? splitres = new string[0];

            string def =
                    "приведите входную строку в соответствие со стандартным видом: \"gen; b; td; err\", где: \n" +
                    "-- gen - строка из степеней элементов порождающего многочлена двоичного поля (степени не должны превышать 8)\n" +
                    "-- b - степень первого корня порождающего многочлена кода\n" +
                    "-- td - количество исправляемых кодом ошибок\n" +
                    "-- err - число симулируемых символьных ошибок передачи\n" +
                    "Входная строка не должна содержать букв или кавычек, только цифры и разделительные знаки ';'\n\n" +
                    "Например, строка \"310; 0; 2; 1\" соответствует коду Рида-Соломона над полем GF(2^3), образованным многочленом x^3 + x + 1;\n" +
                    "У этого кода первым корнем порождающего многочлена является a^0, и он может исправить до двух символьных ошибок.\n" +
                    "При передаче кодового слоыва генерируется одна символьная ошибка.\n";

            try
            {
                splitres = str.Split(';');
            }
            catch
            {
                Output(
                    "Возникла ошибка при обработке строки параметров, " + def
                );
                sourceform.focus();
                return;
            }

            if (splitres.Length < 4)
            {
                Output(
                    "Слишком мало введённых параметров, " + def
                );
                sourceform.focus();
                return;
            }

            int b = 0;
            int td = 1;
            int err = 0;
            try
            {
                b = int.Parse(splitres[1]);
                td = int.Parse(splitres[2]);
                err = int.Parse(splitres[3]);
            }
            catch
            {
                Output(
                     "При парсинге параметров b, td и err произошла ошибка, " + def
                 );
                sourceform.focus();
                return;
            }

            try
            {
                Run(splitres[0], b, td, err);
            }
            catch (System.Exception ex)
            {
                Output(
                    "Возникла неизвестная ошибка, " + def
                );
                sourceform.logexc(ex);
                sourceform.focus();
                return;
            }
        }
    }
}
