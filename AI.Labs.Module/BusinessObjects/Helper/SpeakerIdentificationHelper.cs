// Copyright (c)  2024  Xiaomi Corporation
//
// This file shows how to do speaker identification with sherpa-onnx.
//
// 1. Download a model from
// https://github.com/k2-fsa/sherpa-onnx/releases/tag/speaker-recongition-models
//
// wget https://github.com/k2-fsa/sherpa-onnx/releases/download/speaker-recongition-models/3dspeaker_speech_eres2net_base_sv_zh-cn_3dspeaker_16k.onnx
//
// 2. Download test data from
//
// git clone https://github.com/csukuangfj/sr-data
//
// 3. Now run it
//
// dotnet run

using SherpaOnnx;
using System.Collections.Generic;
using System;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
public class SpeakerIdentificationHelper
{
    public static float[] ComputeEmbedding(SpeakerEmbeddingExtractor extractor, String filename)
    {
        WaveReader reader = new WaveReader(filename);

        OnlineStream stream = extractor.CreateStream();
        stream.AcceptWaveform(reader.SampleRate, reader.Samples);
        stream.InputFinished();

        float[] embedding = extractor.Compute(stream);

        return embedding;
    }

    public static List<int> GetSpeakers(string[] files, double similarityThreshold)
    {
        var config = new SpeakerEmbeddingExtractorConfig();
        config.Model = "d:/model/3dspeaker_speech_campplus_sv_en_voxceleb_16k.onnx";
        config.Debug = 1;
        var extractor = new SpeakerEmbeddingExtractor(config);
        var list = new List<float[]>();
        foreach (var file in files)
        {
            float[] embedding = ComputeEmbedding(extractor, file);
            list.Add(embedding);
        }

        //foreach (var item in list.Select((data, index) => new { data, index }))
        //{
        //    foreach (var i2 in list.Select((data, index) => new { data, index }))
        //    {
        //        if (item.index != i2.index)
        //        {
        //            var sim = CosineSimilarity(item.data, i2.data);
        //            Debug.WriteLine(item.index + "<==>" + i2.index + ":" + sim);
        //        }

        //    }
        //}




        return GetSpeakers(list, similarityThreshold);
    }

    public static int[] Parse(string[] files)
    {
        var config = new SpeakerEmbeddingExtractorConfig();
        config.Model = "d:/model/3dspeaker_speech_campplus_sv_en_voxceleb_16k.onnx";
        config.Debug = 1;
        var extractor = new SpeakerEmbeddingExtractor(config);

        var manager = new SpeakerEmbeddingManager(extractor.Dim);
        //var files = Directory.GetFiles("D:\\VideoInfo\\8\\EnAudioClip", "*.wav");
        var threshold = 0.6f;
        var rst = new int[files.Length];
        int i = 0;
        int index = 0;
        foreach (var file in files)
        {
            float[] embedding = ComputeEmbedding(extractor, file);
            var name = manager.Search(embedding, threshold);
            if (name == "")
            {
                name = $"speaker {i}";
                manager.Add(name, embedding);
                i++;
            }
            rst[index] = i;
            index++;
            Console.WriteLine("{0}: {1}", file, name);
        }

        return rst;
        //string[] spk1Files = files;

        //float[][] spk1Vec = new float[spk1Files.Length][];

        //for (int i = 0; i < spk1Files.Length; ++i)
        //{
        //  spk1Vec[i] = ComputeEmbedding(extractor, spk1Files[i]);
        //}

        //string[] spk2Files =
        //    new string[] {
        //      "D:/sherpa-onnx/dotnet-examples/sr-data-main/enroll/leijun-sr-1.wav", "D:/sherpa-onnx/dotnet-examples/sr-data-main/enroll/leijun-sr-2.wav",
        //    };

        //float[][] spk2Vec = new float[spk2Files.Length][];

        //for (int i = 0; i < spk2Files.Length; ++i)
        //{
        //  spk2Vec[i] = ComputeEmbedding(extractor, spk2Files[i]);
        //}

        //if (!manager.Add("fangjun", spk1Vec))
        //{
        //  Console.WriteLine("Failed to register fangjun");
        //  return;
        //}

        //if (!manager.Add("leijun", spk2Vec))
        //{
        //  Console.WriteLine("Failed to register leijun");
        //  return;
        //}

        //if (manager.NumSpeakers != 2)
        //{
        //  Console.WriteLine("There should be two speakers");
        //  return;
        //}

        //if (!manager.Contains("fangjun"))
        //{
        //  Console.WriteLine("It should contain the speaker fangjun");
        //  return;
        //}

        //if (!manager.Contains("leijun"))
        //{
        //  Console.WriteLine("It should contain the speaker leijun");
        //  return;
        //}

        //Console.WriteLine("---All speakers---");

        //string[] allSpeakers = manager.GetAllSpeakers();

        //foreach (var s in allSpeakers)
        //{
        //  Console.WriteLine(s);
        //}

        //Console.WriteLine("------------");

        //string[] testFiles = files;
        ////    new string[] {
        ////      "D:/sherpa-onnx/dotnet-examples/sr-data-main/test/fangjun-test-sr-1.wav",
        ////      "D:/sherpa-onnx/dotnet-examples/sr-data-main/test/leijun-test-sr-1.wav",
        ////      "D:/sherpa-onnx/dotnet-examples\\sr-data-main/test/liudehua-test-sr-1.wav"
        ////    };

        //float threshold = 0.6f;
        //foreach (var file in testFiles)
        //{
        //  float[] embedding = ComputeEmbedding(extractor, file);
        //  String name = manager.Search(embedding, threshold);
        //  if (name == "")
        //  {
        //    name = "<Unknown>";
        //  }
        //  Console.WriteLine("{0}: {1}", file, name);
        //}

        //// test verify
        //if (!manager.Verify("fangjun", ComputeEmbedding(extractor, testFiles[0]), threshold))
        //{
        //  Console.WriteLine("testFiles[0] should match fangjun!");
        //  return;
        //}

        //if (!manager.Remove("fangjun"))
        //{
        //  Console.WriteLine("Failed to remove fangjun");
        //  return;
        //}

        //if (manager.Verify("fangjun", ComputeEmbedding(extractor, testFiles[0]), threshold))
        //{
        //  Console.WriteLine("{0} should match no one!", testFiles[0]);
        //  return;
        //}

        //if (manager.NumSpeakers != 1)
        //{
        //  Console.WriteLine("There should only 1 speaker left.");
        //  return;
        //}
    }




    //public class Program
    //{
    //    public static void Main()
    //    {
    //        // 示例embedding数据，实际数据应从外部获取
    //        float[][] embeddings = new float[10][];
    //        for (int i = 0; i < 10; i++)
    //        {
    //            embeddings[i] = new float[128]; // 假设每个embedding的维度为128
    //                                            // 填充示例数据
    //            for (int j = 0; j < 128; j++)
    //            {
    //                embeddings[i][j] = i * j * 0.1f;
    //            }
    //        }

    //        // 计算相似度矩阵
    //        double[,] similarityMatrix = new double[10, 10];
    //        for (int i = 0; i < 10; i++)
    //        {
    //            for (int j = 0; j < 10; j++)
    //            {
    //                similarityMatrix[i, j] = CosineSimilarity(embeddings[i], embeddings[j]);
    //            }
    //        }

    //        // 自动选择最佳的聚类数目
    //        int optimalClusters = FindOptimalClusters(similarityMatrix);

    //        // 使用K-means算法进行聚类
    //        int[] clusterLabels = KMeansCluster(similarityMatrix, optimalClusters);

    //        // 输出分组结果
    //        foreach (var label in clusterLabels)
    //        {
    //            Console.WriteLine(label);
    //        }
    //    }

    //    public static double CosineSimilarity(float[] vectorA, float[] vectorB)
    //    {
    //        double dotProduct = 0.0;
    //        double normA = 0.0;
    //        double normB = 0.0;
    //        for (int i = 0; i < vectorA.Length; i++)
    //        {
    //            dotProduct += vectorA[i] * vectorB[i];
    //            normA += Math.Pow(vectorA[i], 2);
    //            normB += Math.Pow(vectorB[i], 2);
    //        }
    //        return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
    //    }

    //    public static int FindOptimalClusters(double[,] similarityMatrix)
    //    {
    //        // 使用轮廓系数评估不同聚类数目的效果
    //        int maxClusters = Math.Min(10, similarityMatrix.GetLength(0)); // 假设最多10个群组或样本数
    //        double bestScore = double.MinValue;
    //        int bestK = 2;

    //        for (int k = 2; k <= maxClusters; k++)
    //        {
    //            int[] labels = KMeansCluster(similarityMatrix, k);
    //            double score = CalculateSilhouetteScore(similarityMatrix, labels);
    //            if (score > bestScore)
    //            {
    //                bestScore = score;
    //                bestK = k;
    //            }
    //        }

    //        return bestK;
    //    }

    //    public static double CalculateSilhouetteScore(double[,] similarityMatrix, int[] labels)
    //    {
    //        int n = similarityMatrix.GetLength(0);
    //        double totalScore = 0.0;

    //        for (int i = 0; i < n; i++)
    //        {
    //            double a = 0.0; // 平均簇内距离
    //            double b = double.MaxValue; // 最小的平均簇间距离

    //            int ownCluster = labels[i];
    //            int clusterCount = 0;
    //            for (int j = 0; j < n; j++)
    //            {
    //                if (i == j) continue;
    //                if (labels[j] == ownCluster)
    //                {
    //                    a += similarityMatrix[i, j];
    //                    clusterCount++;
    //                }
    //                else
    //                {
    //                    double distance = similarityMatrix[i, j];
    //                    b = Math.Min(b, distance);
    //                }
    //            }
    //            a /= clusterCount > 0 ? clusterCount : 1;
    //            double score = (b - a) / Math.Max(a, b);
    //            totalScore += score;
    //        }

    //        return totalScore / n;
    //    }

    //    public static int[] KMeansCluster(double[,] similarityMatrix, int k)
    //    {
    //        int n = similarityMatrix.GetLength(0);
    //        int[] labels = new int[n];
    //        Random random = new Random();
    //        for (int i = 0; i < labels.Length; i++)
    //        {
    //            labels[i] = random.Next(k);
    //        }

    //        bool changed;
    //        do
    //        {
    //            changed = false;
    //            double[][] centroids = new double[k][];
    //            for (int i = 0; i < k; i++)
    //            {
    //                centroids[i] = new double[n];
    //            }

    //            int[] count = new int[k];
    //            for (int i = 0; i < n; i++)
    //            {
    //                int label = labels[i];
    //                for (int j = 0; j < n; j++)
    //                {
    //                    centroids[label][j] += similarityMatrix[i, j];
    //                }
    //                count[label]++;
    //            }

    //            for (int i = 0; i < k; i++)
    //            {
    //                for (int j = 0; j < n; j++)
    //                {
    //                    centroids[i][j] /= count[i] > 0 ? count[i] : 1;
    //                }
    //            }

    //            for (int i = 0; i < n; i++)
    //            {
    //                double maxSimilarity = double.MinValue;
    //                int bestLabel = -1;
    //                for (int j = 0; j < k; j++)
    //                {
    //                    double similarity = CosineSimilarity(embeddings[i], centroids[j]);
    //                    if (similarity > maxSimilarity)
    //                    {
    //                        maxSimilarity = similarity;
    //                        bestLabel = j;
    //                    }
    //                }
    //                if (labels[i] != bestLabel)
    //                {
    //                    labels[i] = bestLabel;
    //                    changed = true;
    //                }
    //            }
    //        } while (changed);

    //        return labels;
    //    }
    //}





    //public static void Main()
    //{
    //    // 示例embedding数据，实际数据应从外部获取
    //    var embeddings = new List<float[]>();
    //    for (var i = 0; i < 10; i++)
    //    {
    //        var embedding = new float[128]; // 假设每个embedding的维度为128
    //        // 填充示例数据
    //        for (var j = 0; j < 128; j++)
    //        {
    //            embedding[j] = i * j * 0.1f;
    //        }
    //        embeddings.Add(embedding);
    //    }

    //    // 实例化Helper类并进行分组
    //    var clusteringHelper = new ClusteringHelper();
    //    var resultLabels = clusteringHelper.ClusterEmbeddings(embeddings, 0.8);

    //    // 输出分组结果
    //    foreach (var label in resultLabels)
    //    {
    //        Console.WriteLine(label);
    //    }
    //}



    // 计算余弦相似度的方法
    private static double CosineSimilarity(float[] vectorA, float[] vectorB)
    {
        var dotProduct = vectorA.Zip(vectorB, (a, b) => a * b).Sum();
        var normA = Math.Sqrt(vectorA.Sum(a => a * a));
        var normB = Math.Sqrt(vectorB.Sum(b => b * b));
        return dotProduct / (normA * normB);
    }

    // 聚类embedding的方法
    public static List<int> GetSpeakers(List<float[]> embeddings, double similarityThreshold)
    {
        var speakerSamples = new List<List<float[]>>();
        var resultLabels = new List<int>();

        foreach (var embedding in embeddings)
        {
            var foundSpeaker = false;
            for (var j = 0; j < speakerSamples.Count; j++)
            {
                if (speakerSamples[j].Any(sample => CosineSimilarity(embedding, sample) > similarityThreshold))
                {
                    resultLabels.Add(j + 1);
                    speakerSamples[j].Add(embedding);
                    foundSpeaker = true;
                    break;
                }
            }
            if (!foundSpeaker)
            {
                speakerSamples.Add(new List<float[]> { embedding });
                resultLabels.Add(speakerSamples.Count);
            }
        }

        return resultLabels;
    }



}
