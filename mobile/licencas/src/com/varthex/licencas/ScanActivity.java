package com.varthex.licencas;

import android.Manifest;
import android.app.Activity;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.hardware.Camera;
import android.os.Bundle;
import android.view.*;
import android.widget.*;
import com.google.zxing.*;
import com.google.zxing.common.HybridBinarizer;
import java.util.*;
import java.util.concurrent.*;

/** Embedded QR decoder: no network, Play Services or external scanner app. */
@SuppressWarnings("deprecation")
public class ScanActivity extends Activity implements SurfaceHolder.Callback {
    private Camera camera;
    private SurfaceView preview;
    private TextView status;
    private boolean resumed, surfaceReady;
    private volatile boolean stopped=true;
    private int session;
    private final ExecutorService decoder=Executors.newSingleThreadExecutor();
    @Override public void onCreate(Bundle state) {
        super.onCreate(state);
        getWindow().addFlags(WindowManager.LayoutParams.FLAG_SECURE);
        LinearLayout body=new LinearLayout(this); body.setOrientation(LinearLayout.VERTICAL); body.setPadding(16,16,16,16);
        status=new TextView(this); status.setText("Aponte para o QR Code na tela de ativação do VComanda."); status.setTextSize(18); status.setAccessibilityLiveRegion(View.ACCESSIBILITY_LIVE_REGION_POLITE); body.addView(status);
        preview=new SurfaceView(this); body.addView(preview,new LinearLayout.LayoutParams(-1,0,1)); preview.getHolder().addCallback(this);
        Button cancel=new Button(this); cancel.setText("Cancelar leitura"); cancel.setMinHeight(56); cancel.setOnClickListener(v->finish()); body.addView(cancel);
        setContentView(body);
        if(checkSelfPermission(Manifest.permission.CAMERA)!=PackageManager.PERMISSION_GRANTED)requestPermissions(new String[]{Manifest.permission.CAMERA},1);
    }
    @Override protected void onResume(){super.onResume();resumed=true;open();}
    @Override protected void onPause(){resumed=false;close();super.onPause();}
    @Override protected void onDestroy(){decoder.shutdownNow();super.onDestroy();}
    @Override public void surfaceCreated(SurfaceHolder h){surfaceReady=true;open();}
    @Override public void surfaceChanged(SurfaceHolder h,int f,int w,int height){}
    @Override public void surfaceDestroyed(SurfaceHolder h){surfaceReady=false;close();}
    @Override public void onRequestPermissionsResult(int r,String[] p,int[] grants){
        super.onRequestPermissionsResult(r,p,grants);
        if(grants.length>0 && grants[0]==PackageManager.PERMISSION_GRANTED)open();
        else status.setText("Câmera não autorizada. Cancele para preencher o código manualmente.");
    }
    private void open(){
        if(!resumed || !surfaceReady || camera!=null || checkSelfPermission(Manifest.permission.CAMERA)!=PackageManager.PERMISSION_GRANTED)return;
        try {
            int id=-1; Camera.CameraInfo info=new Camera.CameraInfo();
            for(int i=0;i<Camera.getNumberOfCameras();i++){Camera.getCameraInfo(i,info);if(info.facing==Camera.CameraInfo.CAMERA_FACING_BACK){id=i;break;}}
            if(id<0)throw new IllegalStateException();
            camera=Camera.open(id);
            Camera.Parameters p=camera.getParameters();
            Camera.Size chosen=null;
            for(Camera.Size s:p.getSupportedPreviewSizes())if(s.width<=1280 && (chosen==null || s.width>chosen.width))chosen=s;
            if(chosen!=null)p.setPreviewSize(chosen.width,chosen.height);
            p.setPreviewFormat(android.graphics.ImageFormat.NV21);
            if(p.getSupportedFocusModes().contains(Camera.Parameters.FOCUS_MODE_CONTINUOUS_PICTURE))p.setFocusMode(Camera.Parameters.FOCUS_MODE_CONTINUOUS_PICTURE);
            camera.setParameters(p);
            int rotation=getWindowManager().getDefaultDisplay().getRotation()*90;
            camera.setDisplayOrientation((info.orientation-rotation+360)%360);
            camera.setPreviewDisplay(preview.getHolder()); stopped=false; camera.startPreview(); next();
        }catch(Exception e){close();status.setText("Não foi possível abrir a câmera. Cancele e use o preenchimento manual.");}
    }
    private void next(){if(camera!=null && !stopped)camera.setOneShotPreviewCallback((bytes,source)->{
        final int current=session;
        Camera.Size size=source.getParameters().getPreviewSize();
        decoder.execute(()->{
            String value=null, error=null;
            try {
                PlanarYUVLuminanceSource luminance=new PlanarYUVLuminanceSource(bytes,size.width,size.height,0,0,size.width,size.height,false);
                Map<DecodeHintType,Object> hints=new EnumMap<>(DecodeHintType.class);
                hints.put(DecodeHintType.POSSIBLE_FORMATS,Collections.singletonList(BarcodeFormat.QR_CODE));
                hints.put(DecodeHintType.TRY_HARDER,Boolean.TRUE);
                Result result=new MultiFormatReader().decode(new BinaryBitmap(new HybridBinarizer(luminance)),hints);
                value=ActivationQr.parse(result.getText());
            }catch(IllegalArgumentException e){error=e.getMessage();}catch(NotFoundException ignored){}
            final String found=value, problem=error;
            runOnUiThread(()->{
                if(stopped || isFinishing() || current!=session)return;
                if(found!=null){setResult(RESULT_OK,new Intent().putExtra("machine",found));close();finish();}
                else {if(problem!=null)status.setText(problem);preview.postDelayed(()->{if(current==session)next();},250);}
            });
        });
    });}
    private void close(){
        stopped=true;session++;
        if(camera!=null){
            try{camera.setPreviewCallback(null);camera.stopPreview();}catch(RuntimeException ignored){}
            finally{camera.release();camera=null;}
        }
    }
}
