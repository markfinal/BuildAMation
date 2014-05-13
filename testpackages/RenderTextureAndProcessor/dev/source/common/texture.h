#ifndef TEXTURE_H
#define TEXTURE_H

struct TextureHeader
{
    void *mpData;
    unsigned int mu32Width;
    unsigned int mu32Height;
    unsigned int mu32TotalTextureDataSize;
};

#endif // TEXTURE_H
