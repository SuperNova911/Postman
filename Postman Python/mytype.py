import ctypes


def int32(value):
    return ctypes.c_int32(value).value


def uint32(value):
    return ctypes.c_uint32(value).value