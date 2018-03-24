using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextClustering.Odin.KCluster.Interfaces;
using TextClustering.Odin.KCluster.Models;
using TextClustering.Odin.KCluster.Utils;

namespace TextClustering.Odin.KCluster.Domain
{
    public class ClusterFactory<T> : IClusterFactory<T> where T : class
    {
        private int _clustersCount;
        private int globalCounter = 0;
        private int _counter;
        private int counter = 0;

        public ClusterFactory()
        {
            _clustersCount = 2;
        }

        public IList<ClusterResult<T>> BuildClusters(IList<DocumentVector<T>> vectors)
        {
            var data = PrepareDocumentCluster(vectors);
            var result = data.Select(x => new ClusterResult<T>
            {
                Documents = x.GroupedDocument
            }).ToList();
            return result;
        }


        public List<Centeroid<T>> PrepareDocumentCluster(IList<DocumentVector<T>> documentCollection)
        {
            globalCounter = 0;
            List<Centeroid<T>> centroidCollection = new List<Centeroid<T>>();
            Centeroid<T> c;
            HashSet<int> uniqRand = new HashSet<int>();
            GenerateRandomNumber(ref uniqRand, _clustersCount, documentCollection.Count);

            foreach (int pos in uniqRand)
            {
                c = new Centeroid<T>();
                c.GroupedDocument = new List<DocumentVector<T>>();
                c.GroupedDocument.Add(documentCollection[pos]);
                centroidCollection.Add(c);
            }
            Boolean stoppingCriteria;
            List<Centeroid<T>> resultSet;
            List<Centeroid<T>> prevClusterCenter;
            InitializeClusterCentroid(out resultSet, centroidCollection.Count);
            do
            {
                prevClusterCenter = centroidCollection;
                foreach (DocumentVector<T> obj in documentCollection)
                {
                    int index = FindClosestClusterCenter(centroidCollection, obj);
                    resultSet[index].GroupedDocument.Add(obj);
                }
                InitializeClusterCentroid(out centroidCollection, centroidCollection.Count());
                centroidCollection = CalculateMeanPoints(resultSet);
                stoppingCriteria = CheckStoppingCriteria(prevClusterCenter, centroidCollection);
                if (!stoppingCriteria)
                {
                    //initialize the result set for next iteration
                    InitializeClusterCentroid(out resultSet, centroidCollection.Count);
                }
            } while (stoppingCriteria == false);
            _counter = counter;
            return resultSet;

        }

        private static void GenerateRandomNumber(ref HashSet<int> uniqRand, int k, int docCount)
        {

            Random r = new Random();

            if (k > docCount)
            {
                do
                {
                    int pos = r.Next(0, docCount);
                    uniqRand.Add(pos);

                } while (uniqRand.Count != docCount);
            }
            else
            {
                do
                {
                    int pos = r.Next(0, docCount);
                    uniqRand.Add(pos);

                } while (uniqRand.Count != k);
            }
        }


        private void InitializeClusterCentroid(out List<Centeroid<T>> centroid, int count)
        {
            Centeroid<T> c;
            centroid = new List<Centeroid<T>>();
            for (int i = 0; i < count; i++)
            {
                c = new Centeroid<T>();
                c.GroupedDocument = new List<DocumentVector<T>>();
                centroid.Add(c);
            }

        }
        private Boolean CheckStoppingCriteria(List<Centeroid<T>> prevClusterCenter, List<Centeroid<T>> newClusterCenter)
        {

            globalCounter++;
            counter = globalCounter;
            if (globalCounter > 11000)
            {
                return true;
            }

            else
            {
                Boolean stoppingCriteria;
                int[] changeIndex = new int[newClusterCenter.Count()]; //1 = centroid has moved 0 == centroid do not moved its position

                int index = 0;
                do
                {
                    int count = 0;
                    if (newClusterCenter[index].GroupedDocument.Count == 0 && prevClusterCenter[index].GroupedDocument.Count == 0)
                    {
                        index++;
                    }
                    else if (newClusterCenter[index].GroupedDocument.Count != 0 && prevClusterCenter[index].GroupedDocument.Count != 0)
                    {
                        for (int j = 0; j < newClusterCenter[index].GroupedDocument[0].VectorSpace.Count(); j++)
                        {
                            //
                            if (newClusterCenter[index].GroupedDocument[0].VectorSpace[j] == prevClusterCenter[index].GroupedDocument[0].VectorSpace[j])
                            {
                                count++;
                            }

                        }

                        if (count == newClusterCenter[index].GroupedDocument[0].VectorSpace.Count())
                        {
                            changeIndex[index] = 0;
                        }
                        else
                        {
                            changeIndex[index] = 1;
                        }
                        index++;
                    }
                    else
                    {
                        index++;
                        continue;

                    }


                } while (index < newClusterCenter.Count());

                // if index list contains 1 stopping criteria is set to flase
                if (changeIndex.Where(s => (s != 0)).Select(r => r).Any())
                {
                    stoppingCriteria = false;
                }
                else
                    stoppingCriteria = true;

                return stoppingCriteria;
            }


        }

         

        private int FindClosestClusterCenter(List<Centeroid<T>> clusterCenter, DocumentVector<T> obj)
        {

            float[] similarityMeasure = new float[clusterCenter.Count()];
            for (int i = 0; i < clusterCenter.Count(); i++)
                similarityMeasure[i] = SimilarityMatrics.FindCosineSimilarity(clusterCenter[i].GroupedDocument[0].VectorSpace, obj.VectorSpace);
            int index = 0;
            float maxValue = similarityMeasure[0];
            for (int i = 0; i < similarityMeasure.Count(); i++)
            {
                if (similarityMeasure[i] > maxValue)
                {
                    maxValue = similarityMeasure[i];
                    index = i;

                }
            }
            return index;
        }
        private List<Centeroid<T>> CalculateMeanPoints(List<Centeroid<T>> _clusterCenter)
        {
            for (int i = 0; i < _clusterCenter.Count(); i++)
            {
                if (_clusterCenter[i].GroupedDocument.Count() > 0)
                {
                    for (int j = 0; j < _clusterCenter[i].GroupedDocument[0].VectorSpace.Count(); j++)
                    {
                        float total = 0;
                        foreach (DocumentVector<T> vSpace in _clusterCenter[i].GroupedDocument)
                            total += vSpace.VectorSpace[j];
                        _clusterCenter[i].GroupedDocument[0].VectorSpace[j] = total / _clusterCenter[i].GroupedDocument.Count();
                    }
                }
            }
            return _clusterCenter;
        }
        public void SetNumberOfClustersRequired(int c) => _clustersCount = c;
    }

}
