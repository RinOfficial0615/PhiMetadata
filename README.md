# PhiMetadata

Tools to decrypt Phigros `game.dat`

## Usage

- Extract `game.dat` and `libUnityPlugin.so` from game apk, then run the following commands.

```bash
$ aarch64-linux-android21-clang++ unityplugin_stub.cpp -O3 -o unityplugin_stub -static-libstdc++ -fvisibility=hidden

$ adb push unityplugin_stub /data/local/tmp
$ adb push /path/to/game.dat /data/local/tmp
$ adb push /path/to/libUnityPlugin.so /data/local/tmp

$ adb shell
marble:/ $ cd /data/local/tmp
marble:/data/local/tmp $ chmod 777 unityplugin_stub
marble:/data/local/tmp $ ./unityplugin_stub ./game.dat ./global-metadata.dat
marble:/data/local/tmp $ exit

$ adb pull /data/local/tmp/global-metadata.dat
```

Note that strings in `global-metadata.dat` are still encrypted. So if you want to use:

- [Il2CppDumper](https://github.com/Perfare/Il2CppDumper/): Run `patch -p1 < /path/to/Il2CppDumper.diff` and rebuild the program yourself.

- [Il2CppInspectorRedux](https://github.com/LukeFZ/Il2CppInspectorRedux): Build and apply [the plugin](./String-Decryptor/).
