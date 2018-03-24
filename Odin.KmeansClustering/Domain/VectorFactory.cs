using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TextClustering.Odin.KCluster.Interfaces;
using TextClustering.Odin.KCluster.Models;

namespace TextClustering.Odin.KCluster.Domain
{
    public class VectorFactory<T> : IVectorFactory<T> where T : class
    {
        private   HashSet<string> distinctTerms;
        private   IList<Document<T>> documentCollection;
        private   Regex r = new Regex("([ \\t{}()\",:;. \n])");    
        public VectorFactory()
        {   
        }  
        public IList<DocumentVector<T>> Build(IList<Document<T>> documents)  
        {             
            distinctTerms = new HashSet<string>();
            documentCollection = documents;  
            foreach (var  documentContent in documents)
            {
                foreach (string term in r.Split(documentContent.ToString()))
                     distinctTerms.Add(term); 
            }   
            List<string> removeList = new List<string>() { "\"", "\r", "\n", "(", ")", "[", "]", "{", "}", "", ".", " ", "," };
            foreach (string s in removeList)
               distinctTerms.Remove(s); 
            List<DocumentVector<T>> documentVectorSpace = new List<DocumentVector<T>>();
            DocumentVector<T> _documentVector;
            float[] space;            
            foreach (var  document in documentCollection)
            {
                int count = 0;
                space = new float[distinctTerms.Count];
                foreach (string term in distinctTerms)
                {
                    space[count] = FindTFIDF(document.ToString(), term);
                    count++;
                }
                _documentVector = new DocumentVector<T>();
                _documentVector.Content = document.GetData();
                _documentVector.VectorSpace = space;
                documentVectorSpace.Add(_documentVector);
            }
            return documentVectorSpace; 
        }
        private   float FindTFIDF(string document, string term)
        {
            float tf = FindTermFrequency(document, term);
            float idf = FindInverseDocumentFrequency(term);
            return tf * idf;
        }   
        private   float FindTermFrequency(string document, string term)
        { 
            int count = r.Split(document).Where(s => s.ToUpper() == term.ToUpper()).Count(); 
            return (float)((float)count / (float)(r.Split(document).Count()));
        }    
        private   float FindInverseDocumentFrequency(string term)
        {
            int count = documentCollection.Select(x=>x.ToString())
                .ToArray().Where(s => r.Split(s.ToUpper()).ToArray().Contains(term.ToUpper())).Count();
            return (float)Math.Log((float)documentCollection.Count() / (float)count); 
        } 
         

    }
}
