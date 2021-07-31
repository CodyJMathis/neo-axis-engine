
��� ��������� � FbxWrapper:
1) � ����� SWIG �������� SWIG\fbxwapper.i, ������ ����� ������������� �������� SWIG\FBXSDK_ChangedHeaders.
2) ��������� SWIG\generate.cmd
��� ���� ������������� ������������ �����:
  - FbxWrapperSln\FbxWrapperNative\FbxWrapperNative.cpp
  - ������� ������������� ��������� FbxWrapperSln\FbxWrapper\*.cs ����� � ��� ����� ������������ �����.
3) ����� �������������� FbxWrapperSln. � ������� FbxWrapperSln\FbxWrapper\FbxWrapper.csproj  ������� ��������� ������:
   <ItemGroup>
    <Compile Include="*.cs" />
  </ItemGroup>
� ���������� ������ ������ ��������� ������, ������������ ��� �����, �� ���� �����. ������� ����� VisualStudio �� ����� � ���� ������� ������ ��������/������� �������.
=================================
==================================
�� ���������� headers:
-------------

1) //%include "fbxsdk/utils/fbxembeddedfilesaccumulator.h" //�����, ��� ������ �� ����� �������������� ������� ������� ���
���� ��� ��������, �� ���� �������� � #include, ����� � *.cpp ����� ��������)
//#include <fbxsdk\utils\fbxembeddedfilesaccumulator.h> 

2) ����� ��� � *lib ����� �������.
%include "fbxsdk/fileio/fbx/fbxreaderfbx5.h"
%include "fbxsdk/fileio/fbx/fbxreaderfbx6.h"
%include "fbxsdk/fileio/fbx/fbxreaderfbx7.h"
%include "fbxsdk/fileio/fbx/fbxwriterfbx5.h"
%include "fbxsdk/fileio/fbx/fbxwriterfbx6.h"
%include "fbxsdk/fileio/fbx/fbxwriterfbx7.h"

3) �� ������� ������ header: //�����: #include <components/libxml2-2.7.8/include/libxml/globals.h>
%include "fbxsdk/fileio/collada/fbxcolladaelement.h" 
%include "fbxsdk/fileio/collada/fbxcolladaiostream.h"
%include "fbxsdk/fileio/collada/fbxcolladanamespace.h" //�����: #include <components/libxml2-2.7.8/include/libxml/globals.h>
%include "fbxsdk/fileio/collada/fbxreadercollada14.h"
%include "fbxsdk/fileio/collada/fbxwritercollada14.h"
%include "fbxsdk/fileio/collada/fbxcolladautils.h"
%include "fbxsdk/fileio/collada/fbxcolladaanimationelement.h"

4) %include "fbxsdk/utils/fbxprocessorxrefuserlib.h"  // C# ����� �������� ������������ override.

5) %include "fbxsdk/fbxsdk_nsbegin.h"
6) ���� �������, �� ���� warning: %include "fbxsdk/fileio/fbxexternaldocreflistener.h" //Multiple inheritance warning 
================================
================================

��������� ������� ���� ����������� � SWIG\FBXSDK_ChangedHeaders:

���������� ��������� ��������� ������:
 - ������ ���������� ������ ������ ����� 
 - ����������� ������� ������������ static,const,...
 - ������� ���� �������, ������� ��������� � const, ���� ��� ����� �� ������� �� ��� const(�� ����������)

------

(��� ��������� ����� ����� FbxLayerElementTemplate.GetDirectArray() ��������� ���������� ���, ����� SWIG ���� warning �� ������, � ������������ ��� ����� � ��������� ������������)
� �����: C:\Program Files\Autodesk\FBX\FBX SDK\2019.0\include\fbxsdk\scene\geometry\fbxlayer.h 
������: 1022
����: template <class Type> class FbxLayerElementTemplate : public FbxLayerElement
�������� ����� ���� ������� �����:
%template(FbxLayerElementArrayTemplateFbxVector4)FbxLayerElementArrayTemplate<FbxVector4>;
%template(FbxLayerElementArrayTemplateFbxSurfaceMaterialPtr)FbxLayerElementArrayTemplate<FbxSurfaceMaterial*>;
%template(FbxLayerElementArrayTemplateFbxVector2)FbxLayerElementArrayTemplate<FbxVector2>;
%template(FbxLayerElementArrayTemplateFbxInt)FbxLayerElementArrayTemplate<int>;
%template(FbxLayerElementArrayTemplateFbxColor)FbxLayerElementArrayTemplate<FbxColor>;
%template(FbxLayerElementArrayTemplateFbxDouble)FbxLayerElementArrayTemplate<double>;
%template(FbxLayerElementArrayTemplateFbxBool)FbxLayerElementArrayTemplate<bool>;
%template(FbxLayerElementArrayTemplateFbxVoidPtr)FbxLayerElementArrayTemplate<void*>;
%template(FbxLayerElementArrayTemplateFbxTexturePtr)FbxLayerElementArrayTemplate<FbxTexture*>;


--------------
� �����: C:\Program Files\Autodesk\FBX\FBX SDK\2019.0\include\fbxsdk\scene\geometry\fbxlayer.h 
������: 1303
����:  class FBXSDK_DLL FbxLayerElementNormal : public FbxLayerElementTemplate<FbxVector4>
�������� ����� ���� ������� �����:
%template(FbxLayerElementTemplateFbxVector4)FbxLayerElementTemplate<FbxVector4>;
%template(FbxLayerElementTemplateFbxSurfaceMaterialPtr)FbxLayerElementTemplate<FbxSurfaceMaterial*>;
%template(FbxLayerElementTemplateFbxVector2)FbxLayerElementTemplate<FbxVector2>;
%template(FbxLayerElementTemplateFbxInt)FbxLayerElementTemplate<int>;
%template(FbxLayerElementTemplateFbxColor)FbxLayerElementTemplate<FbxColor>;
%template(FbxLayerElementTemplateFbxDouble)FbxLayerElementTemplate<double>;
%template(FbxLayerElementTemplateFbxBool)FbxLayerElementTemplate<bool>;
%template(FbxLayerElementTemplateFbxVoidPtr)FbxLayerElementTemplate<void*>;
%template(FbxLayerElementTemplateFbxTexturePtr)FbxLayerElementTemplate<FbxTexture*>;

--------------
� �����: C:\Program Files\Autodesk\FBX\FBX SDK\2019.0\include\fbxsdk\core\base\fbxstringlist.h 
������: 1251
��������: 
%template(FbxStringListTFbxStringListItem) FbxStringListT<FbxStringListItem>;
--------------
� ����� : C:\Program Files\Autodesk\FBX\FBX SDK\2019.0\include\fbxsdk\core\base\fbxmap.h 
������: 433
���������������� ��� 2 ������� (������ ����� �� ���� wirning ��� ��� ignored):
template class FbxSimpleMap<FbxString, FbxObject*, FbxStringCompare>;
template class FbxObjectMap<FbxString, FbxStringCompare>;

������: 366
����: class FBXSDK_DLL FbxObjectStringMap : public FbxObjectMap<FbxString, FbxStringCompare>
�������� ����� ���� �������:
%template(FbxSimpleMapFbxStringFbxObjectPFbxStringCompare) FbxSimpleMap<FbxString, FbxObject*, FbxStringCompare>;
%template(FbxObjectMapFbxStringFbxStringCompare) FbxObjectMap<FbxString, FbxStringCompare>;

