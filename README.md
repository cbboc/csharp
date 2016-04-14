# csharp
C# version of CBBOC


====================
Installing Mono on Ubuntu:

```
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
echo "deb http://download.mono-project.com/repo/debian wheezy main" | sudo tee /etc/apt/sources.list.d/mono-xamarin.list
sudo apt-get update
sudo apt-get install mono-complete
sudo apt-get install monodevelop
```

====================
To Exclude Files from Build in Mono:

1) Right-click file in "Solution" Pad
2) Select "Building Action" > "None"


====================
To Include Files to Build in Mono:

1) Right-click file in "Solution" Pad
2) Select "Building Action" > "Compile"
