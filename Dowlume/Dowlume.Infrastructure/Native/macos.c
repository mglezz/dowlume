#include <fcntl.h>
#include <sys/param.h>

// Keep Darwin's variadic fcntl ABI inside native code (ARM64 uses stack varargs).
int dowlume_get_path(int descriptor, char *buffer)
{
    return fcntl(descriptor, F_GETPATH, buffer);
}
