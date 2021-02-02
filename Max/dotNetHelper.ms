struct ZackdotNetHelper
(
    private
    dotNetAssembly = undefined,
    dotNetActivator = undefined,
    public
    tempDllPath,
    function loadDotNetAssemblyByFile dllFilePath = (
        if (not pathConfig.isAbsolutePath dllFilePath) do (
         throw ("加载dll文件必须使用绝对路径")   
        )
        if not doesFileExist dllFilePath do(
            throw ("dll文件不存在")
        )
        assembly = dotNetClass "System.Reflection.Assembly"
        tempFolder = pathConfig.appendPath (getFilenamePath dllFileName) "Temp/"
        if not doesDirectoryExist tempFolder then(
                makeDir tempFolder
        )else(
            local wildCardName = tempFolder+"*"
            local files = getFiles wildCardName
            for file in files do(
                try (
                    deleteFile file
                ) catch (
                )
            )
        )
        sourceFileName = GetFilenameFile dllFileName
        tempPrefix = (GenClassID returnValue:true)[1] as string
        tempFileName = tempFolder + tempPrefix + sourceFileName + GetFilenameType dllFileName
        CopyFile dllFileName tempFileName
        dotNetActivator = dotNetClass "System.Activator"
        dotNetAssembly = DotNetAssembly.LoadFile tempFileName
    ),
    function loadClass className = (
        _dotNetClass = dotNetClass className
        return _dotNetClass
    ),
    function loadObject className = (
        _dotNetObject = undefined
        try (
            _dotNetType = dotNetAssembly.GetType calssName
            _dotNetObject = dotNetActivator.CreateInstance _dotNetType
        ) catch (
            -- local e = getCurrentException()
            
        )
        return _dotNetObject
    )
)