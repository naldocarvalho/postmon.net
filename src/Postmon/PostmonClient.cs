﻿using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Postmon
{
    public static class PostmonClient
    {
        private const string BASE_URL = "http://api.postmon.com.br/v1/";

        public static Endereco ConsultarCEP(string cep)
        {

            try
            {

                string url = BASE_URL;

                var client = new RestClient(url);


                var request = new RestRequest("cep/{cep}", Method.GET);
                request.AddUrlSegment("cep", cep);


                IRestResponse response = client.Execute(request);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return null;
                }

                var content = response.Content;

                dynamic obj = JsonConvert.DeserializeObject<dynamic>(content);

                Endereco endereco = new Endereco();
                endereco.Bairro = obj.bairro;
                endereco.CEP = obj.cep;
                endereco.Logradouro = obj.logradouro;
                endereco.Unidade = obj.unidade;

                endereco.Cidade = new Cidade();
                endereco.Cidade.Nome = obj.cidade;
                endereco.Cidade.Area = GetDouble((string)obj.cidade_info.area_km2, 0);
                endereco.Cidade.CodigoIBGE = obj.cidade_info.codigo_ibge;

                endereco.Cidade.Estado = new Estado();
                endereco.Cidade.Estado.Nome = obj.estado_info.nome;
                endereco.Cidade.Estado.Sigla = obj.estado;
                endereco.Cidade.Estado.Area = GetDouble((string)obj.estado_info.area_km2, 0);
                endereco.Cidade.Estado.CodigoIBGE = obj.estado_info.codigo_ibge;

                return endereco;
            }
            catch
            {
                return null;
            }
        }

        public static Estado ConsultarEstado(string uf)
        {

            try
            {

                string url = BASE_URL;

                var client = new RestClient(url);


                var request = new RestRequest("uf/{uf}", Method.GET);
                request.AddUrlSegment("uf", uf);


                IRestResponse response = client.Execute(request);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return null;
                }

                var content = response.Content;

                dynamic obj = JsonConvert.DeserializeObject<dynamic>(content);

                Estado estado = new Estado();
                estado.Nome = obj.nome;
                estado.Sigla = uf.ToUpper();
                estado.Area = GetDouble((string)obj.area_km2, 0);
                estado.CodigoIBGE = obj.codigo_ibge;

                return estado;
            }
            catch
            {
                return null;
            }
        }

        public static Cidade ConsultarCidade(string uf, string nome, bool carregarDetalhesDoEstado = false)
        {

            try
            {

                string url = BASE_URL;

                var client = new RestClient(url);


                var request = new RestRequest("cidade/{uf}/{nome}", Method.GET);
                request.AddUrlSegment("uf", uf);
                request.AddUrlSegment("nome", nome);


                IRestResponse response = client.Execute(request);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return null;
                }

                var content = response.Content;

                dynamic obj = JsonConvert.DeserializeObject<dynamic>(content);

                Cidade cidade = new Cidade();
                cidade.Nome = nome;
                cidade.Area = GetDouble((string)obj.area_km2, 0);
                cidade.CodigoIBGE = obj.codigo_ibge;

                if (carregarDetalhesDoEstado)
                {
                    cidade.Estado = ConsultarEstado(uf);
                }
                else
                {
                    cidade.Estado = new Estado();
                    cidade.Estado.Sigla = uf.ToUpper();
                }

                return cidade;
            }
            catch
            {
                return null;
            }
        }


        private static double GetDouble(string value, double defaultValue)
        {
            double result;
            string output;

            // check if last seperator==groupSeperator
            string groupSep = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
            if (value.LastIndexOf(groupSep) + 4 == value.Count())
            {
                bool tryParse = double.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.CurrentCulture, out result);
                result = tryParse ? result : defaultValue;
            }
            else
            {
                // unify string (no spaces, only . )     
                output = value.Trim().Replace(" ", string.Empty).Replace(",", ".");

                // split it on points     
                string[] split = output.Split('.');

                if (split.Count() > 1)
                {
                    // take all parts except last         
                    output = string.Join(string.Empty, split.Take(split.Count() - 1).ToArray());

                    // combine token parts with last part         
                    output = string.Format("{0}.{1}", output, split.Last());
                }
                // parse double invariant     
                result = double.Parse(output, System.Globalization.CultureInfo.InvariantCulture);
            }
            return result;
        }
    }
    
}
