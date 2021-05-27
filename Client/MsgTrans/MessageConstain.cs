using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.MsgTrans
{
    public class MessageConstain
    {
       
    }
    public enum P2PType
    {
        CtoAS=0,    //客户端到AS
        AStoC=1,    //AS到客户端
        CtoTGS=2,   //客户端到TGS
        TGStoC=3,   //TGS到客户端
        CtoS=4,     //客户端到服务器
        StoC=5,      //服务器到客户端
        
    }

    public enum CtoASType
    {

    }

}
