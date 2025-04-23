using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class FileUtils
{
    /// <summary>
    /// 获取目录下视频文件
    /// </summary>
    /// <param name="folderPath"></param>
    /// <param name="isFullName"></param>
    /// <returns></returns>
    private static List<string> GetMovFileNames(string folderPath, Boolean isFullName = true)
    {
        Debug.Log("打开路径" + folderPath);
        List<string> fileNames = new List<string>();
        try
        {
            if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
            {
                // 获取文件夹下的所有图片文件".mp4", ".mov", ".m4v", ".avi", ".wmv", ".webm", ".mkv", ".ts", ".m3u8"
                string[] mediaFiles = Directory.GetFiles(folderPath, "*.mp4")
                .Concat(Directory.GetFiles(folderPath, "*.mov"))
                .Concat(Directory.GetFiles(folderPath, "*.m4v"))
                .Concat(Directory.GetFiles(folderPath, "*.avi"))
                .Concat(Directory.GetFiles(folderPath, "*.wmv"))
                .Concat(Directory.GetFiles(folderPath, "*.wmv"))
                .Concat(Directory.GetFiles(folderPath, "*.webm"))
                .Concat(Directory.GetFiles(folderPath, "*.mkv"))
                .Concat(Directory.GetFiles(folderPath, "*.ts"))
                .Concat(Directory.GetFiles(folderPath, "*.m3u8"))
                .ToArray();

                foreach (string name in mediaFiles)
                {
                    if (isFullName)
                    {
                        fileNames.Add(name);
                    }
                    else
                    {
                        fileNames.Add(Path.GetFileName(name));
                    }
                    // 文件夹名和视频图片文件名添加到列表中

                }
            }
        }
        catch (Exception ex)
        {
            Debug.Log("获取文件列表失败:" + folderPath + " " + ex.Message);
        }


        return fileNames;
    }

    static readonly string[] fileExtensions = { ".mp4", ".mov", ".m4v", ".avi", ".wmv", ".webm", ".mkv", ".ts", ".m3u8", ".jpg", ".jpeg", ".png" }; // 可根据需要添加其他视频格式的扩展名
    /// <summary>
    /// 获取目录和所有子目录下的流媒体文件
    /// </summary>
    /// <param name="folderPath">指定目录</param>
    /// <param name="isFullName">文件是否全名</param>
    /// <returns></returns>
    public static List<string> GetMediaFiles(string folderPath, Boolean isFullName = true)
    {
        List<String> files = GetFilesEach(folderPath, fileExtensions);
        return SortFiles(files, isFullName);
    }

    /// <summary>
    /// 获取目录和所有子目录下的图片文件
    /// </summary>
    /// <param name="folderPath">指定目录</param>
    /// <param name="isFullName">文件是否全名</param>
    /// <returns></returns>
    public static List<string> GetImgFiles(string folderPath, Boolean isFullName = true)
    {
        string[] fileExtensions = { ".jpg", ".jpeg", ".png" }; // 可根据需要添加其他视频格式的扩展名
        List<String> files = GetFilesEach(folderPath, fileExtensions);
        return SortFiles(files, isFullName);
    }

    private static List<string> SortFiles(List<string> files, bool isFullName)
    {
        if (!isFullName)//返回短文件名
        {
            List<String> shortfilenames = new List<String>();
            foreach (string filePath in files)
            {
                shortfilenames.Add(Path.GetFileName(filePath));
            }
            return shortfilenames;
        }
        else
        {
            return files;//返回完整文件名
        }
    }

    private static List<string> GetFilesEach(string folderPath, string[] fileExtensions)
    {
        List<string> files = new List<string>();

        try
        {
            // 遍历当前文件夹中的所有文件
            foreach (string filePath in Directory.GetFiles(folderPath))
            {
                // 检查文件扩展名是否为数组中某种格式
                if (FileExtInArray(filePath, fileExtensions))
                {
                    files.Add(filePath);
                }
            }

            // 遍历当前文件夹中的所有子文件夹
            foreach (string subFolderPath in Directory.GetDirectories(folderPath))
            {
                // 递归调用GetVideoFiles方法，获取子文件夹中的视频文件
                List<string> subFolderVideoFiles = GetFilesEach(subFolderPath, fileExtensions);
                files.AddRange(subFolderVideoFiles);
            }
        }
        catch (Exception ex)
        {
            Debug.Log("读取文件列表异常:" + ex.Message);
        }
        return files;
    }

    private static bool FileExtInArray(string filePath, string[] fileExtensions)
    {
        string fileExtension = Path.GetExtension(filePath).ToLower();
        return Array.Exists(fileExtensions, ext => ext == fileExtension);
    }

    public static List<String> ConvertFullFileNameToShortFolderName(List<String> fullFileNames)
    {
        List<String> result = new List<String>();
        foreach (String fullFileName in fullFileNames)
        {
            String folderpath = Path.GetDirectoryName(fullFileName);
            String shortFolderName = Path.GetFileName(folderpath);
            if (!result.Contains(shortFolderName))
            {
                result.Add(shortFolderName);
            }

        }
        return result;
    }

    public static bool IsImgFile(String fileName)
    {
        bool isimgfile = false;
        if (File.Exists(fileName))
        {
            string extension = Path.GetExtension(fileName).ToLower();
            if (extension == ".jpg" || extension == ".jpeg" || extension == ".png")
            {
                isimgfile = true;
            }
        }
        return isimgfile;
    }

    public static bool IsMovFile(String fileName)
    {
        bool ismovfile = false;
        if (File.Exists(fileName))
        {
            string extension = Path.GetExtension(fileName).ToLower();
            //".mp4", ".mov", ".m4v", ".avi", ".wmv", ".webm", ".mkv", ".ts", ".m3u8"
            if (extension == ".mp4" || extension == ".mov" || extension == ".m4v" || extension == ".avi" || extension == ".wmv" || extension == ".webm" || extension == ".mkv" || extension == ".ts" || extension == ".m3u8")
            {
                ismovfile = true;
            }
        }
        return ismovfile;
    }

    public static string GetShortDirName(String fullFileName)
    {
        String folderpath = Path.GetDirectoryName(fullFileName);
        String shortFolderName = Path.GetFileName(folderpath);
        return shortFolderName;
    }
}
