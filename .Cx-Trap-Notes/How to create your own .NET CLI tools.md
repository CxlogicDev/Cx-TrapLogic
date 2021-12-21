Refereneces: 
-- Nick Chapses Youtube Channel: 
 - https://www.youtube.com/watch?v=JNDgcBDZPkU


 In a Console Project: 
 Add the following 3 PropertyGroup values in the .csproj
 
 /****************************/
    -- in .csproj file
 <PackAsTool>true</PackAsTool>
 <ToolCommandName>{some-Name}</ToolCommandName>
 <PackageOutputPath>{some-[relative or absoluste]-output-path}</PackageOutputPath>

/****************************/

Create Tool Commands: 

- Global 
dotnet tool install --global --add-source {some-[relative or absoluste]-output-path} {console-project-Root-level-Namespace}

