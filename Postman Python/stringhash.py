from mytype import int32, uint32


def sdbm_lower(str):
    if str is None:
        return 0

    str = str.lower()
    value = uint32(0)
    for char in str:
        value = uint32(ord(char) + (int32(value) << 6) + (int32(value) << 16)) - value
    
    return int32(value)