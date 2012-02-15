﻿// C# sharp places this script on root objects of linksets that have more than one child

string buffer;
integer MAX_BUFF_SIZE  = 900;

// Abstraction of actual data transmission
d(string speak) {
    llRegionSay(-4200,speak);
}

default
{
    state_entry()
    {  
        integer n = llGetObjectPrimCount(llGetKey());
       
        integer i;
       
       
        if(n == 1)
        {
            d("Y,1," + (string)llGetKey()+",Z");
            llRemoveInventory(llGetScriptName());   //uncomment this to make script auto remove
            return;
        }
       
        buffer = "Y,"+(string)n;
       
        for(i = 1; i <= n; i++)
        {
            string new = (string)llGetLinkKey(i);
            if (llStringLength(buffer) + llStringLength(new) > MAX_BUFF_SIZE)
            {
                d(buffer);
                buffer = new;
            } else {
                buffer += "," + new;  
            }
        }
        string  new2 = ",Z";
        if (llStringLength(buffer) + llStringLength(new2) > MAX_BUFF_SIZE)
        {
            d(buffer);
            buffer = new2;
        } else {
            buffer += new2;  
        }
        if (buffer != "")d(buffer);
       
        llRemoveInventory(llGetScriptName());   //uncomment this to make script auto remove
    }
}
