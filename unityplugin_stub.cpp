#include <iostream>
#include <fstream>
#include <dlfcn.h>
#include <cstdint>
#include <memory>

constexpr char lib_path[] = "./libUnityPlugin.so";

typedef void * (* def_il2cpp_get_global_metadata) (const char* file_name);

int main(int argc, char* argv[]) {
    if (argc < 3) {
        std::cerr << "Usage: " << argv[0] << "<input metadata file path> <output metadata file path>" << std::endl;
        return 1;
    }

    const char *input_file_path = argv[1];
    const char* output_file_path = argv[2];

    std::cout << "Loading library from path: " << lib_path << std::endl;

    std::unique_ptr<void, decltype(&dlclose)> handle(dlopen(lib_path, RTLD_NOW), dlclose);
    if (!handle) {
        std::cerr << "Failed to open library " << lib_path << ": " << dlerror() << std::endl;
        return 1;
    }
    
    std::cout << "Getting symbol for 'il2cpp_get_global_metadata'..." << std::endl;
    def_il2cpp_get_global_metadata il2cpp_get_global_metadata_func =
        (def_il2cpp_get_global_metadata)dlsym(handle.get(), "_Z26il2cpp_get_global_metadataPKc");

    if (!il2cpp_get_global_metadata_func) {
        std::cerr << "Failed to get symbol 'il2cpp_get_global_metadata': " << dlerror() << std::endl;
        return 1;
    }

    std::cout << "Calling il2cpp_get_global_metadata..." << std::endl;
    void* metadata_ptr = il2cpp_get_global_metadata_func(input_file_path);

    if (!metadata_ptr) {
        std::cerr << "il2cpp_get_global_metadata returned a null pointer." << std::endl;
        return 1;
    }

    std::ifstream input_file(input_file_path, std::ios::binary | std::ios::ate);
    std::streamsize metadata_size = input_file.tellg();
    input_file.close();

    std::cout << "Detected metadata size: " << metadata_size << " bytes." << std::endl;
    std::cout << "Writing metadata to file: " << output_file_path << std::endl;

    std::ofstream output_file(output_file_path, std::ios::binary);
    if (!output_file.is_open()) {
        std::cerr << "Failed to open output file: " << output_file_path << std::endl;
        return 1;
    }

    output_file.write(reinterpret_cast<const char*>(metadata_ptr), metadata_size);
    
    if (output_file.fail()) {
        std::cerr << "Error writing to output file." << std::endl;
        output_file.close();
        return 1;
    }

    output_file.close();
    std::cout << "Successfully wrote " << metadata_size << " bytes of metadata to file." << std::endl;

    return 0;
}