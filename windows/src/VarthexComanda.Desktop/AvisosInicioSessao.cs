namespace VarthexComanda.Desktop;

public sealed class AvisosInicioSessao
{
    private bool _agendado;
    public bool TentarAgendar(bool visivel,bool autenticado,bool bloqueado,bool restauracao)
    {
        if(_agendado||!visivel||!autenticado||bloqueado||restauracao)return false;
        _agendado=true;return true;
    }
    public void Reagendar()=>_agendado=false;
}
