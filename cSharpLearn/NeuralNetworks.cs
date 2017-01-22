using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NeuralNetworks
{
    public class Node
    {
        public Node(int input_num)
        {
            init_w(input_num);

        }
        public void init_w(int input_num)
        {
            w = new double[input_num + 1];
            Random random = new Random();
            for (int i = 0; i < input_num + 1; i++)
            {
                w[i] = (((1.0 * random.Next(0, 100))) / 50 - 1);
            }
        }
        public double[] w;
        public double output;
        static public double operator *(Node node, double[] input)
        {
            double results = 0;
            for (int i = 0; i < node.w.Length - 1; i++)
            {
                results += input[i] * node.w[i];
            }
            results -= node.w[node.w.Length - 1];//最后一个作为阀值
            return (node.output = (1.0 / (1.0 + Math.Exp(-results))));
        }
    }
    public class Ann
    {
        public Ann(int input_num, int hidden_num, int output_num, double rate)
        {
            this.hidden_num = hidden_num;
            this.input_num = input_num;
            this.output_num = output_num;
            this.rate = rate;
            init_w();
        }

        public void init_w()
        {
            //初始化隐藏层
            hidden_node = new Node[hidden_num];
            for (int i = 0; i < hidden_num; i++)
            {
                Node temp = new Node(input_num);
                hidden_node[i] = temp;
            }
            //初始化输出层
            output_node = new Node[output_num];
            for (int i = 0; i < output_num; i++)
            {
                Node temp = new Node(hidden_num);
                output_node[i] = temp;
            }
        }
        public void set_input(double[] input_data)
        {
            this.temp_data = input_data;
            this.last_input_data = input_data;
        }
        public double[] get_output()
        {
            input_to_hidden();
            hidden_to_output();
            if (temp_data.Length != output_num)
            {
                return null;
            }
            double[] temp = temp_data;
            return temp;
        }
        public void train(double[] input_data, double[] answer)
        {
            if (answer.Length != output_num)
            {
                return;
            }
            set_input(input_data);
            var output = get_output();
            double[] output_node_sigema = new double[output_num];//输出层梯度
            double[] hidden_node_sigema = new double[hidden_num];//隐藏层梯度
            for (int i = 0; i < output_num; i++)
            {
                output_node_sigema[i] = output[i] * (1 - output[i]) * (answer[i] - output[i]);
                output_node[i].w[output_node[i].w.Length - 1] -= rate * output_node_sigema[i];//更新输出层阀值
            }
            for (int i = 0; i < output_num; i++)
            {
                for (int j = 0; j < hidden_num; j++)
                {
                    output_node[i].w[j] += rate * output_node_sigema[i] * hidden_node[j].output;//更新输出层权值
                }
            }
            for (int i = 0; i < hidden_num; i++)
            {
                double op = hidden_node[i].output, sigma = 0;
                for (int j = 0; j < output_num; j++)
                {
                    sigma += output_node_sigema[j] * output_node[j].w[i];
                }
                hidden_node_sigema[i] = op * (1 - op) * sigma;
                hidden_node[i].w[hidden_node[i].w.Length - 1] -= rate * hidden_node_sigema[i];//更新隐藏层阀值
            }
            for (int i = 0; i < hidden_num; i++)
            {
                for (int j = 0; j < input_num; j++)
                {
                    hidden_node[i].w[j] += rate * hidden_node_sigema[i] * input_data[j];//更新隐藏层权值
                }
            }
        }
        public void save()
        {
            StreamWriter file = new StreamWriter("info.txt");
            for (int i = 0; i < hidden_num; i++)
            {
                String line = "";
                for (int j = 0; j < input_num + 1; j++)
                    line += Convert.ToString(hidden_node[i].w[j]) + " ";
                line += "\n";
                file.WriteLine(line);
            }
            for (int i = 0; i < output_num; i++)
            {
                String line = "";
                for (int j = 0; j < hidden_num + 1; j++)
                    line += Convert.ToString(output_node[i].w[j]) + " ";
                line += "\n";
                file.WriteLine(line);
            }
            file.Flush();
            file.Close();
        }
        public void load()
        {
            StreamReader file;
            try
            {
                file = new StreamReader("info.txt", Encoding.Default);
            }
            catch (Exception)
            {
                return;
            }
            for (int i = 0; i < hidden_num; i++)
            {
                String line = file.ReadLine();
                if (line.Equals(""))
                {
                    i--;
                    continue;
                }
                for (int j = 0; j < input_num + 1; j++)
                    hidden_node[i].w[j] = double.Parse(line.Split(' ')[j]);
            }
            for (int i = 0; i < output_num; i++)
            {
                String line = file.ReadLine();
                if (line.Equals(""))
                {
                    i--;
                    continue;
                }
                for (int j = 0; j < hidden_num + 1; j++)
                    output_node[i].w[j] = double.Parse(line.Split(' ')[j]);
            }
            file.Close();
        }
        private int input_num;//输入数据数
        private int hidden_num;//隐藏节点数
        private int output_num;//输出节点数量
        private double rate;//学习率
        private double[] temp_data;
        private double[] last_input_data;
        private Node[] hidden_node;
        private Node[] output_node;
        private void input_to_hidden()
        {
            if (temp_data.Length != input_num)
            {
                return;
            }
            double[] temp = new double[hidden_num];
            for (int i = 0; i < hidden_num; i++)
            {
                double results = hidden_node[i] * temp_data;
                temp[i] = results;
            }
            temp_data = temp;
        }
        private void hidden_to_output()
        {
            if (temp_data.Length != hidden_num)
            {
                return;
            }
            double[] temp = new double[output_num];
            for (int i = 0; i < output_num; i++)
            {
                double results = output_node[i] * temp_data;
                temp[i] = results;
            }
            temp_data = temp;
        }

    }
}
